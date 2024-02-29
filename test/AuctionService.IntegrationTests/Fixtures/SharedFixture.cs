using AuctionService.IntegrationTests.Fixtures;

namespace AuctionService.IntegrationTests;

[CollectionDefinition("SharedCollection")]
public class SharedFixture: ICollectionFixture<CustomWebAppFactory>
{

}
