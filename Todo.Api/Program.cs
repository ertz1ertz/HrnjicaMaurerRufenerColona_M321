using Akka.Actor;
using Akka.Cluster.Tools.Singleton;
using Akka.Configuration;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var config = ConfigurationFactory.ParseString(@"
akka {
    actor {
        provider = cluster
    }

    remote {
        dot-netty.tcp {
            hostname = localhost
            port = 8000
        }
    }

    cluster {
        seed-nodes = [""akka.tcp://TodoSystem@localhost:8080""] 
        roles = [""api""]
    }
}");

var system = ActorSystem.Create("TodoSystem", config);
builder.Services.AddSingleton(system);

builder.Services.AddSingleton(provider =>
{
  var sys = provider.GetRequiredService<ActorSystem>();
  var proxy = sys.ActorOf(
    ClusterSingletonProxy.Props(
      singletonManagerPath: "/user/managerSingleton",
      settings: ClusterSingletonProxySettings.Create(sys).WithRole("todo-service")
    ),
    "managerProxy"
  );
  return proxy;
});

builder.Services.AddControllers();
builder.Services.AddCors(opt => opt.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
  c.SwaggerDoc("v1", new OpenApiInfo
  {
    Title = "Todo API",
    Version = "v1",
    Description = "API für das Todo-System auf Basis von Akka.NET"
  });
});

var app = builder.Build();

app.UseCors();

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI(c =>
  {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Todo API v1");
    c.RoutePrefix = string.Empty;
  });
}

app.MapControllers();
app.Run();