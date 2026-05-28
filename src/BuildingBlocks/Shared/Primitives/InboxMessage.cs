namespace Shared.Primitives;

public class InboxMessage
{
    public Guid Id { get; set; }

    public DateTime ProcessedOnUtc { get; set; }
}