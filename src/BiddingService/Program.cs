using MongoDB.Entities;
using MongoDB.Driver;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MassTransit;
using BiddingService.Consumers;
using BiddingService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddHostedService<CheckAuctionFinished>();

builder.Services.AddControllers();

builder.Services.AddMassTransit( options => {
  //configure consumers, add all classes from that namespace that implement IConsumer  
  options.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();

  options.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("bids", false));

  options.UsingRabbitMq( (context,cfg) => 
  {
    cfg.Host(builder.Configuration["RabbitMq:Host"], "/", host => {
      host.Username(builder.Configuration.GetValue("RabbitMq:Username","guest"));
      host.Password(builder.Configuration.GetValue("RabbitMq:Password","guest"));
    });
    cfg.ConfigureEndpoints(context);
  });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer( options =>
{
  options.Authority = builder.Configuration.GetValue<string>("IdentityServiceUrl");
  options.RequireHttpsMetadata = false;
  options.TokenValidationParameters.ValidateAudience = false;
  options.TokenValidationParameters.NameClaimType = "username"; //which claim from the token to use as User Name
});

builder.Services.AddScoped<GrpcAuctionClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

await DB.InitAsync(
  "BidDb",
  MongoClientSettings.FromConnectionString(app.Configuration.GetConnectionString("BidDbConnection"))
);

app.Run();
