using Akka.Actor;
using Akka.Cluster.Tools.Singleton;
using Akka.Configuration;

var builder = Host.CreateDefaultBuilder(args)
  .ConfigureServices((hostContext, services) =>
  {
    var config = ConfigurationFactory.ParseString(@"
akka {
    actor {
        provider = cluster
    }

    remote {
        dot-netty.tcp {
            hostname = localhost
            port = 8080
        }
    }

    cluster {
        seed-nodes = [""akka.tcp://TodoSystem@localhost:8080""] 
        roles = [""todo-service""]
    }
}");

    var system = ActorSystem.Create("TodoSystem", config);
    services.AddSingleton(system);

    var singletonProps = Props.Create(() => new ManagerActor());
    var singleton = ClusterSingletonManager.Props(
      singletonProps,
      terminationMessage: PoisonPill.Instance,
      settings: ClusterSingletonManagerSettings.Create(system).WithRole("todo-service")
    );

    system.ActorOf(singleton, "managerSingleton");

    services.AddHostedService<AkkaHostedService>();
  });

await builder.RunConsoleAsync();

public class AkkaHostedService : IHostedService
{
  private readonly ActorSystem _system;
  public AkkaHostedService(ActorSystem system) => _system = system;
  public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
  public async Task StopAsync(CancellationToken cancellationToken) => await _system.Terminate();
}