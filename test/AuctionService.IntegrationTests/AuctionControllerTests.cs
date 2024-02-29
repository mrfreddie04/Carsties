using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.IntegrationTests.Fixtures;
using AuctionService.IntegrationTests.Util;

namespace AuctionService.IntegrationTests;

[Collection("SharedCollection")]
public class AuctionControllerTests : IAsyncLifetime
{
  private readonly CustomWebAppFactory _factory;
  private readonly HttpClient _httpClient;
  private const string _bugatti_Id = "c8c3ec17-01bf-49db-82aa-1ef80b833a9f";

  public AuctionControllerTests(CustomWebAppFactory factory)
  {
    _factory = factory;
    _httpClient = _factory.CreateClient();
  }

  [Fact]
  public async Task GetAuctions_ShouldReturn3Auctions()
  {
    //arrange
    
    //act
    var response = await _httpClient.GetFromJsonAsync<List<AuctionDto>>("api/auctions");
    
    //assert
    Assert.Equal(3, response.Count);
  }

  [Fact]
  public async Task GetAuctionById_WithValidId_ShouldReturnAuction()
  {
    //arrange
    var id = Guid.Parse(_bugatti_Id);

    //act
    var response = await _httpClient.GetFromJsonAsync<AuctionDto>($"api/auctions/{_bugatti_Id}");
    
    //assert
    Assert.NotNull(response);
    Assert.Equal("Bugatti", response.Make);
    Assert.Equal("Veyron", response.Model);    
  }  

  [Fact]
  public async Task GetAuctionById_WithInValidId_ShouldReturn404()
  {
    //arrange
    var id = Guid.NewGuid();

    //act
    var response = await _httpClient.GetAsync($"api/auctions/{id.ToString()}");
    
    //assert
    Assert.NotNull(response);
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }    

  [Fact]
  public async Task GetAuctionById_WithInValidGuid_ShouldReturn400()
  {
    //arrange
    var id = "notaguid";

    //act
    var response = await _httpClient.GetAsync($"api/auctions/{id}");
    
    //assert
    Assert.NotNull(response);
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }    

  [Fact]
  public async Task CreateAuction_WithNoAuth_ShouldReturn401()
  {
    //arrange
    var auction = new CreateAuctionDto(){ Make="test"};

    //act
    var response = await _httpClient.PostAsJsonAsync("api/auctions",auction);
    
    //assert
    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
  }

  [Fact]
  public async Task CreateAuction_WithValidDtoAndAuth_ShouldReturn201()
  {
    //arrange
    var auction = GetAuctionForCreate();
    //add token to the request
    _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

    //act
    var response = await _httpClient.PostAsJsonAsync("api/auctions",auction);
    
    //assert
    response.EnsureSuccessStatusCode();
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    var createdAution = await response.Content.ReadFromJsonAsync<AuctionDto>();
    Assert.Equal("bob", createdAution.Seller);
  }

  [Fact]
  public async Task CreateAuction_WithInvalidCreateAuctionDto_ShouldReturn400()
  {
    //arrange
    //var auction = "invalid-create-auction-dto";
    var auction = GetAuctionForCreate();
    auction.Make = null;

    //add token to the request
    _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

    //act
    var response = await _httpClient.PostAsJsonAsync("api/auctions",auction);
    
    //assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task UpdateAuction_WithValidUpdateDtoAndUser_ShouldReturn200()
  {
    //arrange
    var auction = GetAuctionForUpdate();
    var id = Guid.Parse(_bugatti_Id);

    //add token to the request
    _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("alice"));

    //act
    var response = await _httpClient.PutAsJsonAsync($"api/auctions/{id}",auction);
    
    //assert
    response.EnsureSuccessStatusCode();
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  public async Task UpdateAuction_WithValidUpdateDtoAndInvalidUser_ShouldReturn403()
  {
    //arrange
    var auction = GetAuctionForUpdate();
    var id = Guid.Parse(_bugatti_Id);

    //add token to the request
    _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("not-alice"));

    //act
    var response = await _httpClient.PutAsJsonAsync($"api/auctions/{id}",auction);
    
    //assert
    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
  }

  public Task DisposeAsync()
  {
    using var scope = _factory.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
    DbHelper.ReinitDbForTests(db);
    return Task.CompletedTask;
  }

  public Task InitializeAsync() => Task.CompletedTask;

  private static CreateAuctionDto GetAuctionForCreate()  
  {
    var auction = new CreateAuctionDto(){ 
      Make="test",
      Model="testModel",
      ImageUrl="test",
      Color="test",
      Mileage=10,
      Year=2000,
      ReservePrice=1000
    };
    return auction;
  }      

  private static UpdateAuctionDto GetAuctionForUpdate()  
  {
    var auction = new UpdateAuctionDto(){ 
      Make="test-update",
      Model="test-update-model",
      Color="test-update",
      Mileage=99,
      Year=1999
    };
    return auction;
  }      
}
