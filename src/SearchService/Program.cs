using System.Net;
using Polly;
using Polly.Extensions.Http;
using SearchService.Data;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

//adds AuctionServiceHttpClient & HttpClient to DI?
builder.Services.AddHttpClient<AuctionServiceHttpClient>()
  .AddPolicyHandler(GetPolicy());

// Add services to the container.
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