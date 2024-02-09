using System;
using System.Collections.Generic;
using System.Linq;
using AuctionService.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AuctionService.Data;
using AuctionService.DTOs;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;

namespace AuctionService.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class AuctionsController : ControllerBase
  {
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionsController(AuctionDbContext context, IMapper mapper, IPublishEndpoint publishEndpoint)
    {
      _context = context;
      _mapper = mapper;
      _publishEndpoint = publishEndpoint;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions([FromQuery] string date) 
    {
      //need to convert to IQueryable type
      var query = _context.Auctions.OrderBy( a => a.Item.Make).AsQueryable();

      if(!string.IsNullOrEmpty(date))
      {
        //convert to UTC time
        var dt = DateTime.Parse(date).AddMilliseconds(1).ToUniversalTime(); 
        //compare to UpdatedAt time
        query = query.Where(a => a.UpdatedAt.CompareTo(dt) > 0);
      }

      var auctionDtos = await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();

      // var auctions = await _context.Auctions
      //   .Include( a => a.Item)
      //   .OrderBy( a => a.Item.Make)
      //   .ToListAsync();
      
      // var auctionsDtos = _mapper.Map<List<AuctionDto>>(auctions);

      return Ok(auctionDtos);
    }

    [HttpGet("{id:Guid}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById([FromRoute] Guid id) {
      var auction = await _context.Auctions
        .Include(a => a.Item)
        .FirstOrDefaultAsync( a => a.Id == id);

      if(auction is null) return NotFound("Auction not found");
      
      var auctionDto = _mapper.Map<AuctionDto>(auction);
      return Ok(auctionDto);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction([FromBody] CreateAuctionDto createAuctionDto)
    {
      //Map dto to Auction entity
      var auction = _mapper.Map<Auction>(createAuctionDto);
      
      //TODO: add current user as seller - this is a protected endpoint now
      //ClaimsPrincipal is authenticated and contains username (retrieved from "username" jwt claim)
      auction.Seller = User.Identity.Name;      

      //track this object in memory
      _context.Auctions.Add(auction);

      var auctionDto = _mapper.Map<AuctionDto>(auction);

      //publish message to all subscribed consumers for the message type (Contracts.AuctionCreated)
      //with outbox - messages are first stored to the DB, so the Publish method is effectively
      //writing to Outox and as such it becomes a part of our EF Transaction, hence we must execute it before
      //we save the changes to the DB
      var auctionCreated = _mapper.Map<AuctionCreated>(auctionDto);
      await _publishEndpoint.Publish(auctionCreated);      

      //ID assigned here
      var result = await _context.SaveChangesAsync() > 0;
      if(!result) return BadRequest("Could not save changes to the DB");

      return CreatedAtAction(
        nameof(GetAuctionById), 
        new {Id = auction.Id},
        auctionDto
      );
    }

    [Authorize]
    [HttpPut("{id:Guid}")]
    public async Task<ActionResult> UpdateAuction([FromRoute] Guid id, [FromBody] UpdateAuctionDto updateAuctionDto)
    {
      //check if exists
      var auction = await _context.Auctions
        .Include( a => a.Item)
        .FirstOrDefaultAsync(a => a.Id == id);

      if(auction is null) return NotFound();

      //TODO: check the seller name matches the user name
      if(auction.Seller != User.Identity.Name) return Forbid();

      //update auction object
      auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
      auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
      auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
      auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;
      auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;

      //publish to message bus
      //var auctionDto = _mapper.Map<AuctionDto>(auction);
      var auctionUpdated = _mapper.Map<AuctionUpdated>(auction);
      await _publishEndpoint.Publish(auctionUpdated);

      //update auction
      var result = await _context.SaveChangesAsync() > 0;
      if(!result) return BadRequest("Problem saving changes");

      //return
      return Ok();      
    }

    [Authorize]
    [HttpDelete("{id:Guid}")]
    public async Task<ActionResult> DeleteAuction([FromRoute] Guid id)
    {
      //check if exists
      var auction = await _context.Auctions.FindAsync(id);

      if(auction is null) return NotFound();

      //TODO: check the seller name matches the user name
      if(auction.Seller != User.Identity.Name) return Forbid();

      //Mark as deleted
      _context.Auctions.Remove(auction);   

      //publish to message bus
      var auctionDeleted = new AuctionDeleted() {
        Id = auction.Id.ToString()
      };
      await _publishEndpoint.Publish(auctionDeleted);

      //Delete from DB
      var result = await _context.SaveChangesAsync() > 0;
      if(!result) return BadRequest("Problem removing auction");

      //return
      return Ok();          
    }    

  }
}