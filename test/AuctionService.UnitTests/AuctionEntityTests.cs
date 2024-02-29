using AuctionService.Entities;

namespace AuctionService.UnitTests;

public class AuctionEntityTests
{
  [Fact]
  //Naming convention - Method_Scenario_ExpectedResult()
  public void HasReservePrice_ReserverPriceGt0_True()
  {
    //Arrange
    var auction = new Auction() {
      Id = Guid.NewGuid(),
      ReservePrice = 10      
    };
    
    //Act
    var result = auction.HasReservePrice();

    //Assert
    Assert.True(result, $"Expected true but received {result}");
  }

  [Fact]
  //Naming convention - Method_Scenario_ExpectedResult()
  public void HasReservePrice_ReserverPriceEq0_False()
  {
    //Arrange
    var auction = new Auction() {
      Id = Guid.NewGuid(),
      ReservePrice = 0     
    };
    
    //Act
    var result = auction.HasReservePrice();

    //Assert
    Assert.False(result, $"Expected false but received {result}");
  }  
}