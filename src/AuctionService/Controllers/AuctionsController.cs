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

namespace AuctionService.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class AuctionsController : ControllerBase
  {
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;

    public AuctionsController(AuctionDbContext context, IMapper mapper )
    {
      _context = context;
      _mapper = mapper;
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

    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction([FromBody] CreateAuctionDto auctionDto)
    {
      //Map dto to Auction entity
      var auction = _mapper.Map<Auction>(auctionDto);
      
      //TODO: add current user as seller
      auction.Seller = "test";      

      //track this object in memory 
      _context.Auctions.Add(auction);

      var result = await _context.SaveChangesAsync() > 0;
      if(!result) return BadRequest("Could not save changes to the DB");

      return CreatedAtAction(
        nameof(GetAuctionById), 
        new {Id = auction.Id},
        _mapper.Map<AuctionDto>(auction)
      );
    }

    [HttpPut("{id:Guid}")]
    public async Task<ActionResult> UpdateAuction([FromRoute] Guid id, [FromBody] UpdateAuctionDto auctionDto)
    {
      //check if exists
      var auction = await _context.Auctions
        .Include( a => a.Item)
        .FirstOrDefaultAsync(a => a.Id == id);

      if(auction is null) return NotFound();

      //TODO: check the seller name matches the user name

      //update auction object
      auction.Item.Make = auctionDto.Make ?? auction.Item.Make;
      auction.Item.Model = auctionDto.Model ?? auction.Item.Model;
      auction.Item.Color = auctionDto.Color ?? auction.Item.Color;
      auction.Item.Year = auctionDto.Year ?? auction.Item.Year;
      auction.Item.Mileage = auctionDto.Mileage ?? auction.Item.Mileage;

      //update auction
      var result = await _context.SaveChangesAsync() > 0;
      if(!result) return BadRequest("Problem saving changes");

      //return
      return Ok();      
    }

    [HttpDelete("{id:Guid}")]
    public async Task<ActionResult> DeleteAuction([FromRoute] Guid id)
    {
      //check if exists
      var auction = await _context.Auctions.FindAsync(id);

      if(auction is null) return NotFound();

      //TODO: check the seller name matches the user name

      //Mark as deleted
      _context.Auctions.Remove(auction);   

      //Delete from DB
      var result = await _context.SaveChangesAsync() > 0;
      if(!result) return BadRequest("Problem removing auction");

      //return
      return Ok();          
    }    

  }
}