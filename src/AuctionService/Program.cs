using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AuctionService.Data;
using MassTransit;
using AuctionService.Consumers;

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

  options.UsingRabbitMq( (context,cfg) => {
    cfg.ConfigureEndpoints(context);
  });
});

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
