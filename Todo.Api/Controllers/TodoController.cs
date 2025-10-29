using Microsoft.AspNetCore.Mvc;
using Akka.Actor;
using Todo.Service.Actors;
using System.Runtime.InteropServices;


[ApiController]
[Route("api/todos")]
public class TodoController : ControllerBase
{
  private readonly IActorRef _manager;

  public TodoController(IActorRef managerProxy)
  {
    _manager = managerProxy;
  }

  [HttpPost]
  public async Task<IActionResult> Create([FromBody] CreateRequest req)
  {
    var id = Guid.NewGuid();
    var cmd = new Messages.CreateTodo(id, req.Title, req.Description);
    var res = await _manager.Ask<object>(cmd, TimeSpan.FromSeconds(5));
    return res switch
    {
      Messages.Ack => CreatedAtAction(nameof(Get), new { id }, new { id }),
      _ => StatusCode(500)
    };
  }

  [HttpGet("{id}")]
  public async Task<IActionResult> Get(Guid id)
  {
    var res = await _manager.Ask<object>(new Messages.GetTodo(id), TimeSpan.FromSeconds(5));
    return res switch
    {
      Messages.TodoState s => Ok(s),
      Messages.NotFound => NotFound(),
      _ => StatusCode(500)
    };
  }

  [HttpPut("{id}")]
  public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRequest req)
  {
    var res = await _manager.Ask<object>(new Messages.UpdateTodo(id, req.Title, req.Description), TimeSpan.FromSeconds(5));
    return res switch
    {
      Messages.Ack => Ok(),
      Messages.NotFound => NotFound(),
      _ => StatusCode(500)
    };
  }


  [HttpPost("{id}/complete")]
  public async Task<IActionResult> Complete(Guid id)
  {
    var res = await _manager.Ask<object>(new Messages.CompleteTodo(id), TimeSpan.FromSeconds(5));
    return res switch
    {
      Messages.Ack => Ok(),
      Messages.NotFound => NotFound(),
      _ => StatusCode(500)
    };
  }


  [HttpDelete("{id}")]
  public async Task<IActionResult> Delete(Guid id)
  {
    var res = await _manager.Ask<object>(new Messages.DeleteTodo(id), TimeSpan.FromSeconds(5));
    return res switch
    {
      Messages.Ack => NoContent(),
      Messages.NotFound => NotFound(),
      _ => StatusCode(500)
    };
  }
}


public record CreateRequest(string Title, string? Description);
public record UpdateRequest(string? Title, string? Description);