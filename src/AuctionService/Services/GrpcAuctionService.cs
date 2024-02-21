using AuctionService.Data;
using AutoMapper;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Services;

public class GrpcAuctionService: GrpcAuction.GrpcAuctionBase
{
  private readonly AuctionDbContext _dbContext;

  public GrpcAuctionService(AuctionDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public override async Task<GrpcAuctionResponse> GetAuction(GetAuctionRequest request, ServerCallContext context)
  {
    Console.WriteLine("===> Received gRPC request for auction");

    var id = request.Id;

    var auction = await _dbContext.Auctions.FindAsync(Guid.Parse(id))
      ?? throw new RpcException(new Status(StatusCode.NotFound,"Auction not found"));

    var response = new GrpcAuctionResponse()
    {
      //Auction =_mapper.Map<GetAuctionModel>(auction) 
      Auction = new GrpcAuctionModel() 
      {
        Id = auction.Id.ToString(),
        Seller = auction.Seller,
        AuctionEnd = auction.AuctionEnd.ToString(),
        ReservePrice = auction.ReservePrice
      }
    };

    return response;
  }
}
