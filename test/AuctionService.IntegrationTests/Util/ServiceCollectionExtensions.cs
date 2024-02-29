using AuctionService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationTests.Util;

public static class ServiceCollectionExtensions
{
  public static void RemoveDbContext<T>(this IServiceCollection services) where T : DbContext
  {
    var descriptor = services.SingleOrDefault( d => d.ServiceType == typeof(DbContextOptions<T>));
    if(descriptor != null) services.Remove(descriptor);
  }

  public static void EnsureCreated<T>(this IServiceCollection services) where T : DbContext
  {
    //Migrate our DB to set up its schema and seed with test data
    var sp = services.BuildServiceProvider();
    using var scope = sp.CreateScope();
    var scopedServices = scope.ServiceProvider;
    var db = scopedServices.GetRequiredService<T>();
    db.Database.Migrate();  

    //Add seed data
    if(db is AuctionDbContext auctionDb)
      DbHelper.InitDbForTests(auctionDb);
  }  
}
