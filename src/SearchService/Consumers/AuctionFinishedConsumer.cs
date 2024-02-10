using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

public class AuctionFinishedConsumer : IConsumer<AuctionFinished>
{
  public async Task Consume(ConsumeContext<AuctionFinished> context)
  {
    Console.WriteLine($"--> Consuming auction finished: {context.Message.AuctionId}");

    //check if exists
    var item = await DB.Find<Item>().OneAsync(context.Message.AuctionId);

    if(item is null) 
      throw new MessageException(typeof(AuctionFinished), "Auction not found"); 

    if(context.Message.ItemSold) 
    {
      //Check the seller name matches the user name
      if(item.Seller != context.Message.Seller) 
        throw new MessageException(typeof(AuctionFinished), "Seller mismatch"); 

      //update auction object
      item.Winner =  context.Message.Winner;
      item.SoldAmount = context.Message.SoldAmount ?? 0;
    }
    
    item.Status = "Finished";

    //update auction
    await item.SaveAsync();   
  }
}
