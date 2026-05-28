namespace OrderService.Application.Common;

public enum CommandResultStatus { Success, NotFound, Forbidden, Conflict, UnprocessableEntity }

public sealed class CommandResult
{
    public CommandResultStatus Status { get; private init; }
    public Guid? CreatedId { get; private init; }
    public string? Error { get; private init; }

    public static CommandResult Success(Guid? id = null) =>
        new() { Status = CommandResultStatus.Success, CreatedId = id };

    public static CommandResult NotFound(string? error = null) =>
        new() { Status = CommandResultStatus.NotFound, Error = error };

    public static CommandResult Forbidden(string? error = null) =>
        new() { Status = CommandResultStatus.Forbidden, Error = error };

    public static CommandResult Conflict(string? error = null) =>
        new() { Status = CommandResultStatus.Conflict, Error = error };

    public static CommandResult UnprocessableEntity(string error) =>
        new() { Status = CommandResultStatus.UnprocessableEntity, Error = error };
}
