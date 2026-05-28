namespace DeliveryService.Application.Common;

public enum CommandResultStatus { Success, NotFound, Forbidden, UnprocessableEntity }

public sealed class CommandResult
{
    public CommandResultStatus Status { get; private init; }
    public string? Error { get; private init; }

    public static CommandResult Success() =>
        new() { Status = CommandResultStatus.Success };

    public static CommandResult NotFound() =>
        new() { Status = CommandResultStatus.NotFound };

    public static CommandResult Forbidden() =>
        new() { Status = CommandResultStatus.Forbidden };

    public static CommandResult UnprocessableEntity(string error) =>
        new() { Status = CommandResultStatus.UnprocessableEntity, Error = error };
}
