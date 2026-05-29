using FluentAssertions;
using NotificationService.Domain.ValueObjects;

namespace NotificationService.Tests;

public class NotificationMessageTests
{
    [Fact]
    public void ForOrderPlaced_ReturnsNonEmptyString()
    {
        var message = NotificationMessage.ForOrderPlaced(Guid.NewGuid());
        message.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void ForPaymentSuccess_ReturnsNonEmptyString()
    {
        var message = NotificationMessage.ForPaymentSuccess(Guid.NewGuid());
        message.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void ForPaymentFailed_ReturnsNonEmptyString()
    {
        var message = NotificationMessage.ForPaymentFailed(Guid.NewGuid());
        message.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void ForOrderDelivered_ReturnsNonEmptyString()
    {
        var message = NotificationMessage.ForOrderDelivered(Guid.NewGuid());
        message.Should().NotBeNullOrWhiteSpace();
    }
}
