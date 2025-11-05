using Akka.Actor;
using Akka.Persistence;

namespace Todo.Service.Actors;

public class TodoActor : PersistentActor
{
  public override string PersistenceId { get; }

  private Messages.TodoState? _state;

  private static readonly TimeSpan IdleTimeout = TimeSpan.FromMinutes(10);

  public TodoActor(string id)
  {
    PersistenceId = $"todo-{id}";

    Context.SetReceiveTimeout(IdleTimeout);
  }

  protected override bool ReceiveCommand(object message)
  {
    switch (message)
    {
      case ReceiveTimeout _:
        Context.Stop(Self); 
        return true;

      case Messages.CreateTodo cmd:
        HandleCreate(cmd);
        return true;

      case Messages.UpdateTodo cmd:
        HandleUpdate(cmd);
        return true;

      case Messages.CompleteTodo cmd:
        HandleComplete(cmd);
        return true;

      case Messages.DeleteTodo cmd:
        HandleDelete(cmd);
        return true;

      case Messages.GetTodo cmd:
        if (_state != null)
          Sender.Tell(_state);
        else
          Sender.Tell(new Messages.NotFound(cmd.Id));
        return true;

      default:
        return false;
    }
  }

  protected override bool ReceiveRecover(object message)
  {
    switch (message)
    {
      case Messages.TodoCreated e:
        Apply(e);
        return true;

      case Messages.TodoUpdated e:
        Apply(e);
        return true;

      case Messages.TodoCompleted e:
        Apply(e);
        return true;

      case Messages.TodoDeleted e:
        Apply(e);
        return true;

      default:
        return false;
    }
  }

  private void HandleCreate(Messages.CreateTodo cmd)
  {
    if (_state != null)
    {
      Sender.Tell(new Messages.Ack(cmd.Id));
      return;
    }

    var evt = new Messages.TodoCreated(cmd.Id, cmd.Title, cmd.Description);
    Persist(evt, e =>
    {
      Apply(e);
      Sender.Tell(new Messages.Ack(cmd.Id));
    });
  }

  private void HandleUpdate(Messages.UpdateTodo cmd)
  {
    if (_state == null)
    {
      Sender.Tell(new Messages.NotFound(cmd.Id));
      return;
    }

    var evt = new Messages.TodoUpdated(cmd.Id, cmd.Title, cmd.Description);
    Persist(evt, e =>
    {
      Apply(e);
      Sender.Tell(new Messages.Ack(cmd.Id));
    });
  }

  private void HandleComplete(Messages.CompleteTodo cmd)
  {
    if (_state == null)
    {
      Sender.Tell(new Messages.NotFound(cmd.Id));
      return;
    }

    var evt = new Messages.TodoCompleted(cmd.Id);
    Persist(evt, e =>
    {
      Apply(e);
      Sender.Tell(new Messages.Ack(cmd.Id));
    });
  }

  private void HandleDelete(Messages.DeleteTodo cmd)
  {
    if (_state == null)
    {
      Sender.Tell(new Messages.NotFound(cmd.Id));
      return;
    }

    var evt = new Messages.TodoDeleted(cmd.Id);
    Persist(evt, e =>
    {
      Apply(e);
      Sender.Tell(new Messages.Ack(cmd.Id));
    });
  }

  private void Apply(Messages.TodoCreated e)
  {
    _state = new Messages.TodoState(e.Id, e.Title, e.Description, false);
  }

  private void Apply(Messages.TodoUpdated e)
  {
    if (_state == null) return;
    var title = e.Title ?? _state.Title;
    var desc = e.Description ?? _state.Description;
    _state = new Messages.TodoState(e.Id, title, desc, _state.Completed);
  }

  private void Apply(Messages.TodoCompleted e)
  {
    if (_state == null) return;
    _state = new Messages.TodoState(e.Id, _state.Title, _state.Description, true);
  }

  private void Apply(Messages.TodoDeleted e)
  {
    _state = null;
  }
}
