using AutoMapper;
using BiddingService.DTOs;
using BiddingService.Models;
using Contracts;

namespace BiddingService.RequestHelpers;

public class MappingProfiles: Profile
{
  public MappingProfiles()
  {
    CreateMap<AuctionCreated,Auction>();
    CreateMap<AuctionUpdated,Auction>();
    CreateMap<Bid,BidDto>();
    
    CreateMap<Bid,BidPlaced>()
      .ForMember(
        dest => dest.BidId,
        opt => opt.MapFrom( src => src.ID)
      );    
  }
}