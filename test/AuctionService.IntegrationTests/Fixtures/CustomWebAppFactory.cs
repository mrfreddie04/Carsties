using AuctionService.Data;
using AuctionService.IntegrationTests.Util;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using WebMotions.Fake.Authentication.JwtBearer;

namespace AuctionService.IntegrationTests.Fixtures;

public class CustomWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
  private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();

  public async Task InitializeAsync()
  {    
    await _postgreSqlContainer.StartAsync();
  }

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    //base.ConfigureWebHost(builder);
    builder.ConfigureTestServices( services => {
      //1) Postgres DB
      services.RemoveDbContext<AuctionDbContext>();

      // add test DbContext service
      services.AddDbContext<AuctionDbContext>(options => {
        options.UseNpgsql(_postgreSqlContainer.GetConnectionString());
      });

      //2) RabbitMQ - we can use test harness provides out of the box by MQ  
      services.AddMassTransitTestHarness();

      //3) Migrate our DB to set up its schema and seed with test data
      services.EnsureCreated<AuctionDbContext>();

      //4) Add Fake Token to request
      services.AddAuthentication(FakeJwtBearerDefaults.AuthenticationScheme).AddFakeJwtBearer( options =>
      {
        options.BearerValueType = FakeJwtBearerBearerValueType.Jwt;
      });

    });
  }

  Task IAsyncLifetime.DisposeAsync() => _postgreSqlContainer.DisposeAsync().AsTask();
}
