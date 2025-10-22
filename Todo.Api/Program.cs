using Akka.Actor;
using Akka.Configuration;


var builder = WebApplication.CreateBuilder(args);


var config = ConfigurationFactory.ParseString(@"akka { actor.provider = local }");
var system = ActorSystem.Create("TodoSystem", config);


builder.Services.AddSingleton(system);
builder.Services.AddControllers();


var app = builder.Build();
app.MapControllers();
app.Run();