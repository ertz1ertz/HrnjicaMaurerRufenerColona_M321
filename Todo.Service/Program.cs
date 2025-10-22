using Akka.Actor;
using Akka.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Todo.Service.Actors;
using Microsoft.Extensions.Configuration;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
      config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    })
    .ConfigureServices((hostContext, services) =>
    {
      var akkaHocon = hostContext.Configuration.GetSection("Akka").GetChildren();
      // simplify: build minimal HOCON wired to SQLite plugin via appsettings values
      var config = @"akka {
            actor.provider = local
            persistence {
              journal { plugin = ""akka.persistence.journal.sqlite"" }
              snapshot-store { plugin = ""akka.persistence.snapshot-store.sqlite"" }
            }
        }";

      var actorSystem = ActorSystem.Create("TodoSystem", ConfigurationFactory.ParseString(config));
      services.AddSingleton(actorSystem);

      // create a top-level actor manager
      services.AddSingleton(provider =>
      {
        var sys = provider.GetRequiredService<ActorSystem>();
        var manager = sys.ActorOf(Props.Create(() => new ManagerActor()), "manager");
        return manager;
      });

      services.AddHostedService<AkkaHostedService>();
    });

await builder.RunConsoleAsync();


// Hosted service to keep ActorSystem lifetime with host
public class AkkaHostedService : IHostedService
{
  private readonly ActorSystem _system;
  public AkkaHostedService(ActorSystem system) => _system = system;
  public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
  public async Task StopAsync(CancellationToken cancellationToken) => await _system.Terminate();
}

// Manager actor that creates child TodoActors on demand
public class ManagerActor : ReceiveActor
{
  public ManagerActor()
  {
    Receive<Messages.CreateTodo>(cmd => GetOrCreateActor(cmd.Id).Forward(cmd));
    Receive<Messages.UpdateTodo>(cmd => GetOrCreateActor(cmd.Id).Forward(cmd));
    Receive<Messages.CompleteTodo>(cmd => GetOrCreateActor(cmd.Id).Forward(cmd));
    Receive<Messages.DeleteTodo>(cmd => GetOrCreateActor(cmd.Id).Forward(cmd));
    Receive<Messages.GetTodo>(cmd => GetOrCreateActor(cmd.Id).Forward(cmd));
  }

  private IActorRef GetOrCreateActor(Guid id)
  {
    var name = $"todo-{id}";
    var child = Context.Child(name);
    if (child.IsNobody())
    {
      child = Context.ActorOf(Props.Create(() => new TodoActor(id.ToString())), name);
    }
    return child;
  }
}
