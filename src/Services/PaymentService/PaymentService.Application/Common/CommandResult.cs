namespace PaymentService.Application.Common;

public enum CommandResultStatus { Success, NotFound, Forbidden }

public sealed class CommandResult
{
    public CommandResultStatus Status { get; private init; }
    public string? Error { get; private init; }

    public static CommandResult Success() =>
        new() { Status = CommandResultStatus.Success };

    public static CommandResult NotFound(string? error = null) =>
        new() { Status = CommandResultStatus.NotFound, Error = error };

    public static CommandResult Forbidden(string? error = null) =>
        new() { Status = CommandResultStatus.Forbidden, Error = error };
}
