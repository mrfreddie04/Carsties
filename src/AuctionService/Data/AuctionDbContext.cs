using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionService.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data
{
  public class AuctionDbContext : DbContext
  {
    public AuctionDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Auction> Auctions { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
      base.OnModelCreating(modelBuilder);

      //needed for MassTransit.Outbox configuration - add DbSets to maintaing MT Outbox/Inbox entities
      modelBuilder.AddInboxStateEntity();
      modelBuilder.AddOutboxStateEntity();
      modelBuilder.AddOutboxMessageEntity();
    }
  }
}