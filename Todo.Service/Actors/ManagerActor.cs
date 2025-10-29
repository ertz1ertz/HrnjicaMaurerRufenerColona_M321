using Akka.Actor;
using Todo.Service.Actors;

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