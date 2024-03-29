using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using AutoMapper;
using MassTransit;
using AuctionService.Data;
using AuctionService.Consumers;
using AuctionService.Services;
using Polly;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AuctionDbContext>(options => {
  options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

//builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddControllers();

builder.Services.AddMassTransit( options => {
  //configure consumers, add all classes from that namespace that implement IConsumer  
  options.AddConsumersFromNamespaceContaining<AuctionCreatedFaultConsumer>();

  options.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", false));

  //setup outbox
  options.AddEntityFrameworkOutbox<AuctionDbContext>( (IEntityFrameworkOutboxConfigurator o) => {
    //every 10 seconds retry to send messages from Outbox to ServiceBus
    o.QueryDelay = TimeSpan.FromSeconds(10);
    //which database type to use to store outbox - configure to use with Postgres
    o.UsePostgres();
    //send messges via outbox (not directly)
    o.UseBusOutbox();
  });

  options.UsingRabbitMq( (context,cfg) => 
  {
    cfg.UseMessageRetry(r => {
      r.Handle<RabbitMqConnectionException>();
      r.Interval(5, TimeSpan.FromSeconds(10));
    });
    
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

builder.Services.AddGrpc();

builder.Services.AddScoped<IAuctionRepository,AuctionRepository>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

//map controllers - direct http requests to correct api endpoint
app.MapControllers();

app.MapGrpcService<GrpcAuctionService>();

//create retry policy
var retryPolicy = Policy
  .Handle<NpgsqlException>()
  .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(10));

//apply migrations & seed data
retryPolicy.ExecuteAndCapture( () => app.InitDb());

//apply migrations & seed data
// try
// {
//   app.InitDb();
// }
// catch(Exception e) 
// {
//   Console.WriteLine(e);
//   //throw;
// }

app.Run();

public partial class Program { }