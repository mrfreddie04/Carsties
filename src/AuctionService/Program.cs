using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AuctionService.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AuctionDbContext>(options => {
  options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddDbContext<AuctionDbContext>(options => {
  options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

//builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthorization();

//map controllers - direct http requests to correct api endpoint
app.MapControllers();

//apply migrations & seed data
try
{
  app.InitDb();
}
catch(Exception e) 
{
  Console.WriteLine(e);
  //throw;
}

app.Run();
