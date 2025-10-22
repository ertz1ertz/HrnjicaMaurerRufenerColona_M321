namespace Todo.Service.Actors;

public static class Messages
{
    public record CreateTodo(Guid Id, string Title, string? Description);
    public record UpdateTodo(Guid Id, string? Title, string? Description);
    public record CompleteTodo(Guid Id);
    public record DeleteTodo(Guid Id);
    public record GetTodo(Guid Id);

    public record TodoCreated(Guid Id, string Title, string? Description);
    public record TodoUpdated(Guid Id, string? Title, string? Description);
    public record TodoCompleted(Guid Id);
    public record TodoDeleted(Guid Id);

    public record TodoState(Guid Id, string Title, string? Description, bool Completed);
    public record NotFound(Guid Id);
    public record Ack(Guid Id);
}