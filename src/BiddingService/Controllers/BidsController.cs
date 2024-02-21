using AutoMapper;
using BiddingService.DTOs;
using BiddingService.Models;
using BiddingService.Services;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;

namespace BiddingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BidsController: ControllerBase
{
  private readonly IMapper _mapper;
  private readonly IPublishEndpoint _publishEndpoint;
  private readonly GrpcAuctionClient _grpcAuctionClient;

  public BidsController(IMapper mapper, IPublishEndpoint publishEndpoint, GrpcAuctionClient grpcAuctionClient)
  {
    _mapper = mapper;
    _publishEndpoint = publishEndpoint;
    _grpcAuctionClient = grpcAuctionClient;
  }

  [HttpGet("{auctionId}")]
  public async Task<ActionResult<List<BidDto>>> GetBidsForAuction(string auctionId) 
  {
    var bids = await DB.Find<Bid>()
                .Match(bid => bid.AuctionId == auctionId)
                .Sort( sb => sb.Descending(bid => bid.BidTime))
                .ExecuteAsync();  
    //return Ok(_mapper.Map<List<BidDto>>(bids));
    return Ok(bids.Select(bid => _mapper.Map<BidDto>(bid)).ToList());
  }

  [Authorize]
  [HttpPost]
  public async Task<ActionResult<BidDto>> PlaceBid(string auctionId, int amount) 
  {
    var auction = await DB.Find<Auction>().OneAsync(auctionId);
    if(auction == null) 
    {
      auction = _grpcAuctionClient.GetAuction(auctionId);
      if(auction == null) return BadRequest("Cannot bid on this auction at this time");
      await DB.SaveAsync(auction);
    }

    if(auction.Seller == User.Identity.Name)
    {
      return BadRequest("You cannot bid on your auction");
    }

    var bid = new Bid() 
    {
      AuctionId = auctionId,
      Amount = amount,
      Bidder = User.Identity.Name,
    };

    //calculate BidStatus
    if(auction.AuctionEnd < DateTime.UtcNow) 
    {
      bid.BidStatus = BidStatus.Finished;  
    } 
    else  
    {
      var highBid = await DB.Find<Bid>()
            .Match(bid => bid.AuctionId == auctionId)
            .Sort( sb => sb.Descending(bid => bid.Amount))
            .ExecuteFirstAsync();  
      var highBidAmount = highBid?.Amount ?? 0;     

      if(highBidAmount >= bid.Amount)
      {
        bid.BidStatus = BidStatus.TooLow;
      }
      else 
      {
        bid.BidStatus = bid.Amount < auction.ReservePrice ? BidStatus.AcceptedBelowReserve : BidStatus.Accepted;
      }
    }

    await DB.SaveAsync(bid);

    var bidPlaced = _mapper.Map<BidPlaced>(bid);
    await _publishEndpoint.Publish(bidPlaced);      

    return Ok(_mapper.Map<BidDto>(bid));
  }    
}
