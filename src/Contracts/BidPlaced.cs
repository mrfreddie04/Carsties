﻿namespace Contracts;

public class BidPlaced
{
  public string BidId { get; set; }  
  public string AuctionId { get; set; }
  public string Bidder { get; set; }
  public int Amount { get; set; }
  public DateTime BidTime { get; set; }
  public string BidStatus { get; set; } //accepted,rejected,...
}
