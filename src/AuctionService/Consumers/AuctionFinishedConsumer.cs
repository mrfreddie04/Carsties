using AuctionService.Data;
using AuctionService.Entities;
using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Consumers;

public class AuctionFinishedConsumer : IConsumer<AuctionFinished>
{
  private readonly AuctionDbContext _dbContext;

  public AuctionFinishedConsumer(AuctionDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task Consume(ConsumeContext<AuctionFinished> context)
  {
    Console.WriteLine($"--> Consuming auction finished: {context.Message.AuctionId}");

    //check if exists
    if(!Guid.TryParse(context.Message.AuctionId, out var id))
      throw new MessageException(typeof(AuctionFinished), "Invalid Auction Id"); 
    
    var auction = await _dbContext.Auctions.FindAsync(id);

    if(auction is null) 
      throw new MessageException(typeof(AuctionFinished), "Auction not found"); 

    if(context.Message.ItemSold) 
    {
      //Check the seller name matches the user name
      if(auction.Seller != context.Message.Seller) 
        throw new MessageException(typeof(AuctionFinished), "Seller mismatch"); 

      //update auction object
      auction.Winner =  context.Message.Winner;
      auction.SoldAmount = context.Message.SoldAmount;
      auction.Status = Status.Finished;
    }
    else 
    {
      auction.Status = Status.ReserveNotMet;
    }

    //update auction
    await _dbContext.SaveChangesAsync();
  }
}
