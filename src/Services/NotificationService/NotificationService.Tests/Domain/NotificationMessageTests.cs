using FluentAssertions;
using NotificationService.Domain.ValueObjects;

namespace NotificationService.Tests.Domain;

public class NotificationMessageTests
{
    [Fact]
    public void ForOrderPlaced_ReturnsNonEmptyString()
        => NotificationMessage.ForOrderPlaced(Guid.NewGuid()).Should().NotBeNullOrWhiteSpace();

    [Fact]
    public void ForPaymentSuccess_ReturnsNonEmptyString()
        => NotificationMessage.ForPaymentSuccess(Guid.NewGuid()).Should().NotBeNullOrWhiteSpace();

    [Fact]
    public void ForPaymentFailed_ReturnsNonEmptyString()
        => NotificationMessage.ForPaymentFailed(Guid.NewGuid()).Should().NotBeNullOrWhiteSpace();

    [Fact]
    public void ForOrderDelivered_ReturnsNonEmptyString()
        => NotificationMessage.ForOrderDelivered(Guid.NewGuid()).Should().NotBeNullOrWhiteSpace();
}
