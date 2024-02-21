using MassTransit;
using NotificationService.Consumers;
using NotificationService.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit( options => {
  //configure consumers, add all classes from that namespace that implement IConsumer  
  options.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();

  options.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("nt", false));

  options.UsingRabbitMq( (context,cfg) => 
  {
    cfg.Host(builder.Configuration["RabbitMq:Host"], "/", host => {
      host.Username(builder.Configuration.GetValue("RabbitMq:Username","guest"));
      host.Password(builder.Configuration.GetValue("RabbitMq:Password","guest"));
    });
    cfg.ConfigureEndpoints(context);
  });
});

builder.Services.AddSignalR();

var app = builder.Build();

app.MapHub<NotificationHub>("/notifications");

app.Run();
