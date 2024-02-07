using System.Net;
using MassTransit;
using Polly;
using Polly.Extensions.Http;
using SearchService;
using SearchService.Consumers;
using SearchService.Data;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAutoMapper(typeof(Program));

//add AuctionServiceHttpClient & HttpClient to DI?
builder.Services.AddHttpClient<AuctionServiceHttpClient>()
  .AddPolicyHandler(GetPolicy());

//add MassTransit to the DI
builder.Services.AddMassTransit( options => 
{
  //configure consumers, add all classes from that namespace that implement IConsumer  
  options.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();

  //specify prefix to be used in front of the queue name to create different
  //queues for different services that consume the same event
  //false - do not include the namespace in the formatted name
  //"Consumer" suffix is dropped, so AuctionCreatedConsumer => search-auction-created
  options.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));

  //set up default connection to RabbitMQ (localhost:5672)  
  options.UsingRabbitMq( (context,cfg) => 
  {
    //configure retry policy, specify RabbitMQ endpoint 
    cfg.ReceiveEndpoint("search-auction-created", e => 
    {
      e.UseMessageRetry( r => r.Interval(5, 5) ); //retry 5 times, with 5 seconds wait time in between
      e.ConfigureConsumer<AuctionCreatedConsumer>(context); //apply to AuctionCreatedConsumer consumer
    });

    //configure endpoints for all defined consumers - automatically assigning names to the queues
    cfg.ConfigureEndpoints(context);
  });
});

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseAuthorization();

app.MapControllers();

//initialize DB, create indexes, seed data
app.Lifetime.ApplicationStarted.Register(
  async () => {
    try 
    {
      await DbInitializer.InitDb(app);
    }
    catch(Exception e)
    {
      Console.WriteLine(e);
    }
  }
);

app.Run();

static IAsyncPolicy<HttpResponseMessage> GetPolicy()
{
  return HttpPolicyExtensions
    .HandleTransientHttpError() //returns a policy builder to configure policy to handle http request that fail due to transient conditions
    .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound) //handle also NotFound response
		.WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3)); //returns a policy that will retry indefinitely every 3 seconds
}