using BiddingService.Models;
using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace BiddingService.Services;

public class CheckAuctionFinished : BackgroundService
{
  private readonly ILogger<CheckAuctionFinished> _logger;
  private readonly IServiceProvider _services;

  public CheckAuctionFinished(ILogger<CheckAuctionFinished> logger, IServiceProvider services)
  {
    _logger = logger;
    _services = services;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _logger.LogInformation("Starting check for finished auctions");
    //use stoppingToken to detect when app is shutting down
    stoppingToken.Register(() => _logger.LogInformation("===> Auction check is stopping"));

    while(!stoppingToken.IsCancellationRequested)
    {
      await CheckAuctions(stoppingToken);
      await Task.Delay(5000, stoppingToken);
    }
  }

  private async Task CheckAuctions(CancellationToken stoppingToken)
  {
    var finishedAuctions = await DB.Find<Auction>()
            .Match(a => a.AuctionEnd < DateTime.UtcNow && !a.Finished)
            .ExecuteAsync(stoppingToken);  

    if(finishedAuctions.Count == 0) return;       

    _logger.LogInformation("===> Found {count} auctions that have completed", finishedAuctions.Count);

    await using var scope = _services.CreateAsyncScope();

    var endpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

    foreach(var auction in finishedAuctions)
    {
      auction.Finished = true;
      await auction.SaveAsync(cancellation: stoppingToken);

      //get winning bid
      var winningBid = await DB.Find<Bid>()
            .Match(bid => bid.AuctionId == auction.ID)
            .Match(bid => bid.BidStatus == BidStatus.Accepted)
            .Sort( sb => sb.Descending(bid => bid.Amount))
            .ExecuteFirstAsync(stoppingToken);  

      var auctionFinished = new AuctionFinished()
      {
        AuctionId = auction.ID,
        Seller = auction.Seller,
        ItemSold = winningBid != null,
        Winner = winningBid?.Bidder,
        SoldAmount = winningBid?.Amount
      };

      await endpoint.Publish(auctionFinished, stoppingToken);    
    }    
  }
}
