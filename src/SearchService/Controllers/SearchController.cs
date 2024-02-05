using DnsClient.Protocol;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.RequestHelpers;
using ZstdSharp.Unsafe;

namespace SearchService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController: ControllerBase
{
  [HttpGet]
  public async Task<ActionResult<List<Item>>> SearchItems(
    [FromQuery] SearchParams searchParams
  )
  {
    var query = DB.PagedSearch<Item,Item>();
    
    //add search term
    if(!string.IsNullOrEmpty(searchParams.SearchTerm))
    {
      query.Match(Search.Full,searchParams.SearchTerm).SortByTextScore();
    }

    query = searchParams.OrderBy switch 
    {
      "make" => query.Sort(sb => sb.Ascending(item => item.Make)),
      "new" => query.Sort(sb => sb.Descending(item => item.CreatedAt)),
      _ => query.Sort(sb => sb.Ascending(item => item.AuctionEnd))
    };

    query = searchParams.FilterBy switch 
    {
      "finished" => query.Match(item => item.AuctionEnd < DateTime.UtcNow),
      "endingSoon" => query.Match(item => item.AuctionEnd > DateTime.UtcNow 
        && item.AuctionEnd < DateTime.UtcNow.AddHours(6)),
      _ => query.Match(item => item.AuctionEnd > DateTime.UtcNow) //live auctions
    };

    if(!string.IsNullOrEmpty(searchParams.Seller))
    {
      query.Match(item => item.Seller == searchParams.Seller);
    }

    if(!string.IsNullOrEmpty(searchParams.Winner))
    {
      query.Match(item => item.Winner == searchParams.Winner);
    }    

    query.PageSize(searchParams.PageSize);
    query.PageNumber(searchParams.PageNumber);

    var result = await query.ExecuteAsync();
    return Ok(new {
      result.Results,
      result.TotalCount,
      result.PageCount
    });
  }
}
