using AuctionService;
using BiddingService.Models;
using Grpc.Net.Client;
using MassTransit.SagaStateMachine;

namespace BiddingService.Services;

public class GrpcAuctionClient
{
  private readonly ILogger<GrpcAuctionClient> _logger;
  private readonly IConfiguration _config;

  public GrpcAuctionClient(ILogger<GrpcAuctionClient> logger, IConfiguration config)
  {
    _logger = logger;
    _config = config;
  }

  public Auction GetAuction(string id)
  {
    _logger.LogInformation("===> Calling GRPC Service");

    //create grpc request
    var address = _config.GetValue<string>("GrpcAuction");
    var channel = GrpcChannel.ForAddress(address);
    var client = new GrpcAuction.GrpcAuctionClient(channel);
    var request = new GetAuctionRequest(){Id = id};

    try 
    {
      var reply = client.GetAuction(request);
      var auction = new Auction() 
      {
        ID = reply.Auction.Id,
        Seller = reply.Auction.Seller,
        ReservePrice = reply.Auction.ReservePrice,
        AuctionEnd = DateTime.Parse(reply.Auction.AuctionEnd)
      };
      return auction;
    } 
    catch(Exception ex)
    {
      _logger.LogError(ex, "Could not call GRPC Server");
      return null;
    }
  }
}
