using System.Text.Json;
using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.Services;

namespace SearchService.Data;

public class DbInitializer
{
  public static async Task InitDb(WebApplication app)
  {
    //initialize DB
    await DB.InitAsync(
      "SearchDb",
      MongoClientSettings.FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection"))
    );

    //Create index for our Item collection - because we use this collection for search server
    //we arbitrarily pick 3 fields to create index on - to make searching on make, model & color 
    await DB.Index<Item>()
      .Key(item => item.Make, KeyType.Text)
      .Key(item => item.Model, KeyType.Text)
      .Key(item => item.Color, KeyType.Text)
      .CreateAsync();  

    //seed data
    // var count = await DB.CountAsync<Item>();
    // if(count == 0) 
    // { 
    //   Console.WriteLine("No data - will attempt to seed");

    //   //read data as text - path is relative to the project root?
    //   var data = await File.ReadAllTextAsync("Data/auctions.json");

    //   //deserialize into .Net type - List<Item>
    //   var options = new JsonSerializerOptions() {
    //     //ignore case when matching prop names from the json object with props from the target Item class
    //     PropertyNameCaseInsensitive=true,
    //   };
    //   var items = JsonSerializer.Deserialize<List<Item>>(data, options);

    //   await DB.SaveAsync<Item>(items);
    // }

    using var scope = app.Services.CreateScope();
		var httpClient = scope.ServiceProvider.GetRequiredService<AuctionServiceHttpClient>();
    var items = await httpClient.GetItemsForSearchDb();

    Console.WriteLine($"Items fetched from Auction Service: {items.Count}");

    if(items.Count > 0) {
      await DB.SaveAsync<Item>(items);
    }
  }
}
