using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

public class BidPlacedConsumer : IConsumer<BidPlaced>
{
  public async Task Consume(ConsumeContext<BidPlaced> context)
  {
    Console.WriteLine($"--> Consuming bid placed: {context.Message.AuctionId}");

    var item = await DB.Find<Item>().OneAsync(context.Message.AuctionId);

    if(item is null) 
      throw new MessageException(typeof(AuctionFinished), "Auction not found"); 

    //update auction object
    if(context.Message.BidStatus.Contains("Accepted") &&
      context.Message.Amount > item.CurrentHighBid ) 
    {
      item.CurrentHighBid =  context.Message.Amount;
      await item.SaveAsync();
    }
  }
}

