using AutoFixture;
using MassTransit;
using Moq;
using AuctionService.Controllers;
using AuctionService.Data;
using AutoMapper;
using AuctionService.RequestHelpers;
using AuctionService.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;
using AuctionService.Entities;
using Contracts;
using AuctionService.UnitTests.Utils;
using Microsoft.AspNetCore.Http;

namespace AuctionService.UnitTests;

public class AuctionControllerTests
{
  private readonly Mock<IAuctionRepository> _auctionRepo;
  private readonly Mock<IPublishEndpoint> _publishEndpoint;
  private readonly Fixture _fixture;
  private readonly AuctionsController _controller;
  private readonly IMapper _mapper;

  public AuctionControllerTests()
  {
    _fixture = new Fixture();
    _auctionRepo = new Mock<IAuctionRepository>();
    _publishEndpoint = new Mock<IPublishEndpoint>();

    //create actual mapper (based on AuctionService mapping configuration)
    var mockMapper = new MapperConfiguration( mc => {
      mc.AddMaps(typeof(MappingProfiles).Assembly); //use actual maps from our AuctionService
    }).CreateMapper().ConfigurationProvider;
    _mapper = new Mapper(mockMapper);

    //create controller - to be tested
    _controller = new AuctionsController(
      _auctionRepo.Object, 
      _mapper, 
      _publishEndpoint.Object
    ) {
      ControllerContext = new ControllerContext() {
        HttpContext = new DefaultHttpContext() {
          User = Helpers.GetClaimsPrincipal()
        }
      }  
    };
    //_controller.ControllerContext.HttpContext.User = Helpers.GetClaimsPrincipal();
  }

  [Fact]
  //Naming convention - Method_Scenario_ExpectedResult()
  public async Task GetAllAuctions_WithNoParams_Returns10Auctions()
  {
    //Arrange
    //create fake auctions
    var auctions = _fixture.CreateMany<AuctionDto>(10).ToList();
    //setup repo mock to return these auctions when called w/out data
    _auctionRepo.Setup( repo => repo.GetAuctionsAsync(null)).ReturnsAsync(auctions);

    //Act
    var result = await _controller.GetAllAuctions(null);
    var okResult = result.Result as OkObjectResult;

    //Assert
    Assert.NotNull(result);
    Assert.Equal(10, (okResult.Value as List<AuctionDto>).Count);
    //Assert.Equal(10, result.Value.Count);
    Assert.IsType<ActionResult<List<AuctionDto>>>(result);
  }  

  [Fact]
  public async Task GetAuctionById_WithValidGuid_ReturnsAuction()
  {
    //Arrange
    //create fake auctions
    var auction = _fixture.Create<AuctionDto>();
    //setup repo mock to return this auction, we do not care if the passed param matches the GUID of auction in the repo
    _auctionRepo.Setup( repo => repo.GetAuctionByIdAsync(It.IsAny<Guid>()))
      .ReturnsAsync(auction);

    //Act
    var result = await _controller.GetAuctionById(auction.Id);

    //Assert
    Assert.Equal(auction.Id, result.Value.Id);
    Assert.Equal(auction.Make, result.Value.Make);
    Assert.IsType<ActionResult<AuctionDto>>(result);
  }    

  [Fact]
  public async Task GetAuctionById_WithInValidGuid_ReturnsNotFound()
  {
    //Arrange
    //setup repo mock to return this auction, we do not care if the passed param matches the GUID of auction in the repo
    _auctionRepo.Setup( repo => repo.GetAuctionByIdAsync(It.IsAny<Guid>()))
      .ReturnsAsync(value: null);

    //Act
    var result = await _controller.GetAuctionById(Guid.NewGuid());

    //Assert
    Assert.IsType<NotFoundResult>(result.Result);
  }   

  [Fact]
  public async Task CreateAuction_WithValidCreateAuctionDto_ReturnsCreatedAtActionResult()
  {
    //Arrange
    var auction = _fixture.Create<CreateAuctionDto>();    
    //if a method returns void - we do not need to chain .Returns()
    _auctionRepo.Setup( repo => repo.AddAuction(It.IsAny<Auction>()));
    _auctionRepo.Setup( repo => repo.SaveChangesAsync()).ReturnsAsync(true);
    _publishEndpoint.Setup( mq => mq.Publish(It.IsAny<AuctionCreated>(), new CancellationToken()));    

    //Act
    var result = await _controller.CreateAuction(auction);
    var createdResult = result.Result as CreatedAtActionResult;

    //Assert
    Assert.NotNull(createdResult);
    Assert.Equal("GetAuctionById",createdResult.ActionName);
    Assert.IsType<AuctionDto>(createdResult.Value);
  }    

  [Fact]
  public async Task CreateAuction_FailedSave_Returns400BadRequest()
  {
    //Arrange
    var auction = _fixture.Create<CreateAuctionDto>();    
    //if a method returns void - we do not need to chain .Returns()
    _auctionRepo.Setup( repo => repo.AddAuction(It.IsAny<Auction>()));
    _auctionRepo.Setup( repo => repo.SaveChangesAsync()).ReturnsAsync(false);
    _publishEndpoint.Setup( mq => mq.Publish(It.IsAny<AuctionCreated>(), new CancellationToken()));    

    //Act
    var result = await _controller.CreateAuction(auction);

    //Assert
    Assert.IsType<BadRequestObjectResult>(result.Result);
  } 

  [Fact]
  public async Task UpdateAuction_WithUpdateAuctionDto_ReturnsOkResponse()
  {
    //Arrange
    var updateAuctionDto = _fixture.Create<UpdateAuctionDto>();   

    // var auction = _fixture.Build<Auction>()
    //   .With( a => a.Item, new Item())
    //   .With( a => a.Id, id)
    //   .With( a => a.Seller, _controller.User.Identity.Name)
    //   .Create();  

    //to avoid circular refrences 
    var auction = _fixture.Build<Auction>()
      .Without( a => a.Item)
      .With( a => a.Seller, _controller.User.Identity.Name)
      .Create();        
    auction.Item = _fixture.Build<Item>().Without(i => i.Auction).Create();  

    _auctionRepo.Setup(repo => repo.GetAuctionEntityByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);
    _auctionRepo.Setup( repo => repo.SaveChangesAsync()).ReturnsAsync(true);
    _publishEndpoint.Setup( mq => mq.Publish(It.IsAny<AuctionUpdated>(), new CancellationToken()));         

    //Act
    var result = await _controller.UpdateAuction(auction.Id, updateAuctionDto);

    //Assert
    Assert.IsType<OkResult>(result);
  }  

  [Fact]
  public async Task UpdateAuction_WithInvalidUser_Returns403Forbid()
  {
    //Arrange
    var updateAuctionDto = _fixture.Create<UpdateAuctionDto>();   
    
    //to avoid circular refrences 
    var auction = _fixture.Build<Auction>()
      .Without( a => a.Item)
      .With( a => a.Seller, "invalid")
      .Create();        
    //not needed - execution should stop before we access Item prop  
    //auction.Item = _fixture.Build<Item>().Without(i => i.Auction).Create();  

    _auctionRepo.Setup(repo => repo.GetAuctionEntityByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);
    //not needed - execution should stop before we executed these methods
    // _auctionRepo.Setup( repo => repo.SaveChangesAsync()).ReturnsAsync(true);
    // _publishEndpoint.Setup( mq => mq.Publish(It.IsAny<AuctionUpdated>(), new CancellationToken()));         

    //Act
    var result = await _controller.UpdateAuction(auction.Id, updateAuctionDto);

    //Assert
    Assert.IsType<ForbidResult>(result);
  }  

  [Fact]
  public async Task UpdateAuction_WithInvalidGuid_ReturnsNotFound()
  {
    //Arrange
    var updateAuctionDto = _fixture.Create<UpdateAuctionDto>();   
    _auctionRepo.Setup(repo => repo.GetAuctionEntityByIdAsync(It.IsAny<Guid>()))
      .ReturnsAsync(value: null);

    //Act
    var result = await _controller.UpdateAuction(It.IsAny<Guid>(), updateAuctionDto);

    //Assert
    Assert.IsType<NotFoundResult>(result);
  }

  [Fact]
  public async Task DeleteAuction_WithValidUser_ReturnsOkResponse()
  {
    //Arrange
    var auction = _fixture.Build<Auction>()
      .Without( a => a.Item)
      .With( a => a.Seller, _controller.User.Identity.Name)
      .Create();        
    //auction.Item = _fixture.Build<Item>().Without(i => i.Auction).Create();  

    _auctionRepo.Setup(repo => repo.GetAuctionEntityByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);
    _auctionRepo.Setup(repo => repo.RemoveAuction(It.IsAny<Auction>()));
    _auctionRepo.Setup( repo => repo.SaveChangesAsync()).ReturnsAsync(true);
    _publishEndpoint.Setup( mq => mq.Publish(It.IsAny<AuctionUpdated>(), new CancellationToken()));         

    //Act
    var result = await _controller.DeleteAuction(auction.Id);

    //Assert
    Assert.IsType<OkResult>(result);
  }

  [Fact]
  public async Task DeleteAuction_WithInvalidGuid_Returns404Response()
  {
    //Arrange
    var id = Guid.NewGuid();
    _auctionRepo.Setup(repo => repo.GetAuctionEntityByIdAsync(id))
      .ReturnsAsync(value: null);      

    //Act
    var result = await _controller.DeleteAuction(id);

    //Assert
    Assert.IsType<NotFoundResult>(result);
  }

  [Fact]
  public async Task DeleteAuction_WithInvalidUser_Returns403Response()
  {
    //Arrange
    var id = Guid.NewGuid();

    //to avoid circular refrences 
    var auction = _fixture.Build<Auction>()
      .Without( a => a.Item)
      .With( a => a.Seller, "invalid")
      .Create();        

    _auctionRepo.Setup(repo => repo.GetAuctionEntityByIdAsync(id)).ReturnsAsync(auction);

    //Act
    var result = await _controller.DeleteAuction(id);

    //Assert
    Assert.IsType<ForbidResult>(result);
  }  

}
