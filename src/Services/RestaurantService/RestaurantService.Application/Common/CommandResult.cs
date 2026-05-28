namespace RestaurantService.Application.Common;

public enum CommandResultStatus { Success, NotFound, Forbidden, Conflict }

public sealed class CommandResult
{
    public CommandResultStatus Status { get; private init; }
    public Guid? CreatedId { get; private init; }

    public static CommandResult Success(Guid? id = null) =>
        new() { Status = CommandResultStatus.Success, CreatedId = id };

    public static CommandResult NotFound() =>
        new() { Status = CommandResultStatus.NotFound };

    public static CommandResult Forbidden() =>
        new() { Status = CommandResultStatus.Forbidden };

    public static CommandResult Conflict() =>
        new() { Status = CommandResultStatus.Conflict };
}
