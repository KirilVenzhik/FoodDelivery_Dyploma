namespace DeliveryService.Application.Common;

public sealed class QueryResult<T> where T : class
{
    public T? Value { get; private init; }
    public CommandResultStatus Status { get; private init; }

    public static QueryResult<T> Ok(T value) =>
        new() { Value = value, Status = CommandResultStatus.Success };

    public static QueryResult<T> NotFound() =>
        new() { Status = CommandResultStatus.NotFound };
}
