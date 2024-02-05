using Microsoft.AspNetCore.Http.HttpResults;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Services;

public class AuctionServiceHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public AuctionServiceHttpClient(HttpClient httpClient, IConfiguration config)
    {
      _httpClient = httpClient;
      _config = config;
    }  

    public async Task<List<Item>> GetItemsForSearchDb()
    {
      var lastUpdated = await DB.Find<Item, string>()
        .Sort(sb => sb.Descending(item => item.UpdatedAt))
        .Project( item => item.UpdatedAt.ToString())
        .ExecuteFirstAsync();

      var auctionServiceUrl = _config.GetValue<string>("AuctionServiceUrl");  

      var items = await _httpClient.GetFromJsonAsync<List<Item>>($"{auctionServiceUrl}/api/auctions?date={lastUpdated}");  

      return items;  
    }  
}
