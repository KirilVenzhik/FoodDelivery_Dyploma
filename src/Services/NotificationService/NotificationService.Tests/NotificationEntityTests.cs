using FluentAssertions;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.Events;

namespace NotificationService.Tests;

public class NotificationEntityTests
{
    private static Notification CreateNotification() =>
        Notification.Create(Guid.NewGuid(), Guid.NewGuid(), "Test message", NotificationType.OrderPlaced);

    [Fact]
    public void Create_WithValidData_SetsIsReadFalse()
    {
        var notification = CreateNotification();
        notification.IsRead.Should().BeFalse();
    }

    [Fact]
    public void Create_WithValidData_SetsSentAtNull()
    {
        var notification = CreateNotification();
        notification.SentAt.Should().BeNull();
    }

    [Fact]
    public void Create_RaisesNotificationCreatedDomainEvent()
    {
        var notification = CreateNotification();
        notification.DomainEvents.Should().ContainSingle(e => e is NotificationCreatedDomainEvent);
    }

    [Fact]
    public void MarkAsSent_SetsSentAt()
    {
        var notification = CreateNotification();
        notification.ClearDomainEvents();

        notification.MarkAsSent();

        notification.SentAt.Should().NotBeNull();
        notification.SentAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void MarkAsSent_RaisesNotificationSentDomainEvent()
    {
        var notification = CreateNotification();
        notification.ClearDomainEvents();

        notification.MarkAsSent();

        notification.DomainEvents.Should().ContainSingle(e => e is NotificationSentDomainEvent);
    }

    [Fact]
    public void MarkAsRead_SetsIsReadTrue()
    {
        var notification = CreateNotification();

        notification.MarkAsRead();

        notification.IsRead.Should().BeTrue();
    }

    [Fact]
    public void MarkAsRead_RaisesNotificationReadDomainEvent()
    {
        var notification = CreateNotification();
        notification.ClearDomainEvents();

        notification.MarkAsRead();

        notification.DomainEvents.Should().ContainSingle(e => e is NotificationReadDomainEvent);
    }
}
