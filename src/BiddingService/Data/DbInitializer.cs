using MongoDB.Driver;
using MongoDB.Entities;

namespace BiddingService.Data;

public class DbInitializer
{
  public static async Task InitDb(WebApplication app)
  {
    //initialize DB
    await DB.InitAsync(
      "BidDb",
      MongoClientSettings.FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection"))
    );
  }  
}
