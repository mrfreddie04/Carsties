using AutoMapper;
using BiddingService.Models;
using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace BiddingService.Consumers;

public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
{
  // private readonly IMapper _mapper;

  // public AuctionCreatedConsumer(IMapper mapper)
  // {
  //   _mapper = mapper;
  // }

  public async Task Consume(ConsumeContext<AuctionCreated> context)
  {
    Console.WriteLine($"--> Consuming auction created: {context.Message.Id}");

    //we specifically receive object of Contracts.AuctionCreated type, we need to map it to Item
    //var auction = _mapper.Map<Auction>(context.Message);
    var auction = new Auction()
    {
      ID = context.Message.Id,
      AuctionEnd = context.Message.AuctionEnd,
      Seller = context.Message.Seller,
      ReservePrice = context.Message.ReservePrice
    };

    //save to DB
    await auction.SaveAsync();
  }
}
