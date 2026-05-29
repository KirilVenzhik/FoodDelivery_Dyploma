using FluentAssertions;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.Events;

namespace NotificationService.Tests.Domain;

public class NotificationEntityTests
{
    private static Notification MakeNotification()
        => Notification.Create(Guid.NewGuid(), Guid.NewGuid(), "Test message", NotificationType.OrderPlaced);

    [Fact]
    public void Create_WithValidData_SetsIsReadFalse()
    {
        var n = MakeNotification();
        n.IsRead.Should().BeFalse();
    }

    [Fact]
    public void Create_WithValidData_SetsSentAtNull()
    {
        var n = MakeNotification();
        n.SentAt.Should().BeNull();
    }

    [Fact]
    public void Create_RaisesNotificationCreatedDomainEvent()
    {
        var n = MakeNotification();
        n.DomainEvents.Should().ContainSingle(e => e is NotificationCreatedDomainEvent);
    }

    [Fact]
    public void MarkAsSent_SetsSentAt()
    {
        var n = MakeNotification();
        n.MarkAsSent();
        n.SentAt.Should().NotBeNull();
        n.SentAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void MarkAsSent_RaisesNotificationSentDomainEvent()
    {
        var n = MakeNotification();
        n.ClearDomainEvents();
        n.MarkAsSent();
        n.DomainEvents.Should().ContainSingle(e => e is NotificationSentDomainEvent);
    }

    [Fact]
    public void MarkAsRead_SetsIsReadTrue()
    {
        var n = MakeNotification();
        n.MarkAsRead();
        n.IsRead.Should().BeTrue();
    }

    [Fact]
    public void MarkAsRead_RaisesNotificationReadDomainEvent()
    {
        var n = MakeNotification();
        n.ClearDomainEvents();
        n.MarkAsRead();
        n.DomainEvents.Should().ContainSingle(e => e is NotificationReadDomainEvent);
    }
}
