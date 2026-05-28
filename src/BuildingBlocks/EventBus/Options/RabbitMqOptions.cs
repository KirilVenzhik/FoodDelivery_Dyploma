namespace EventBus.Options;

public class RabbitMqOptions
{
    public string Host { get; set; } = default!;

    public string Username { get; set; } = default!;

    public string Password { get; set; } = default!;

    public string ExchangeName { get; set; } = default!;
}