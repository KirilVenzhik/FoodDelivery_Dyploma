namespace NotificationService.Application.Common;

public enum CommandResultStatus { Success, NotFound, Forbidden }

public sealed class CommandResult
{
    public CommandResultStatus Status { get; private init; }

    public static CommandResult Success() =>
        new() { Status = CommandResultStatus.Success };

    public static CommandResult NotFound() =>
        new() { Status = CommandResultStatus.NotFound };
}
