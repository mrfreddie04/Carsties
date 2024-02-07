using AutoMapper;
using AuctionService.DTOs;
using AuctionService.Entities;
using Contracts;

namespace AuctionService.RequestHelpers;

public class MappingProfiles: Profile
{
  public MappingProfiles()
  {
    //Auction => AuctionDto
    //Flatten two entities into a single object
    CreateMap<Auction,AuctionDto>()
      .IncludeMembers( a => a.Item)
      .ForMember(
        dest => dest.Status,
        opt => opt.MapFrom( src => src.Status.ToString())
      );      
    CreateMap<Item,AuctionDto>();

    //CreateAuctionDto => Auction & Item (the related entity)
    CreateMap<CreateAuctionDto,Auction>()
      .ForMember(
        dest => dest.Item,
        opt => opt.MapFrom( src => src)
      );
    //need another map to tell AutoMapper how to map CreateAuctionDto to Item  
    CreateMap<CreateAuctionDto,Item>();		

    CreateMap<AuctionDto,AuctionCreated>();
    CreateMap<AuctionDto,AuctionUpdated>();

    CreateMap<Auction,AuctionUpdated>()
      .IncludeMembers( a => a.Item);      
    CreateMap<Item,AuctionUpdated>();

  }
}
