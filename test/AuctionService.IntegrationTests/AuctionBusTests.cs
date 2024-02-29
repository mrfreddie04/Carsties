using System.Net;
using System.Net.Http.Json;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.IntegrationTests.Fixtures;
using AuctionService.IntegrationTests.Util;
using Contracts;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationTests;

[Collection("SharedCollection")]
public class AuctionBusTests : IClassFixture<CustomWebAppFactory>, IAsyncLifetime
{
  private readonly CustomWebAppFactory _factory;
  private readonly HttpClient _httpClient;
  private readonly ITestHarness _testHarness;

  public AuctionBusTests(CustomWebAppFactory factory)
  {
    _factory = factory;
    _httpClient = _factory.CreateClient();
    _testHarness = _factory.Services.GetTestHarness();
  }

  [Fact]
  public async Task CreateAuction_WithValidObjectAndAuth_ShouldPublishAuctionCreated()
  {
    //arrange
    var auction = GetAuctionForCreate();
    //add token to the request
    _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

    //act
    var response = await _httpClient.PostAsJsonAsync("api/auctions", auction);
    
    //assert
    response.EnsureSuccessStatusCode();
    Assert.True(await _testHarness.Published.Any<AuctionCreated>());
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

}
