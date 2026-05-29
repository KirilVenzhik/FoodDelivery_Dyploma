using FluentAssertions;
using DeliveryService.Domain.Entities;
using DeliveryService.Domain.Enums;
using DeliveryService.Domain.Events;
using DeliveryService.Domain.Exceptions;

namespace DeliveryService.Tests;

public class DeliveryAggregateTests
{
    private static Delivery CreateDelivery() =>
        Delivery.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

    [Fact]
    public void Create_WithValidData_SetsStatusPending()
    {
        var delivery = CreateDelivery();
        delivery.Status.Should().Be(DeliveryStatus.Pending);
    }

    [Fact]
    public void Create_RaisesDeliveryCreatedDomainEvent()
    {
        var delivery = CreateDelivery();
        delivery.DomainEvents.Should().ContainSingle(e => e is DeliveryCreatedDomainEvent);
    }

    [Fact]
    public void Assign_WhenPending_SetsStatusAssigned()
    {
        var delivery = CreateDelivery();
        delivery.Assign(Guid.NewGuid());
        delivery.Status.Should().Be(DeliveryStatus.Assigned);
    }

    [Fact]
    public void Assign_WhenPending_SetsCourierId()
    {
        var delivery = CreateDelivery();
        var courierId = Guid.NewGuid();
        delivery.Assign(courierId);
        delivery.CourierId.Should().Be(courierId);
    }

    [Fact]
    public void Assign_RaisesDeliveryAssignedDomainEvent()
    {
        var delivery = CreateDelivery();
        delivery.ClearDomainEvents();
        delivery.Assign(Guid.NewGuid());
        delivery.DomainEvents.Should().ContainSingle(e => e is DeliveryAssignedDomainEvent);
    }

    [Fact]
    public void Assign_WhenAlreadyAssigned_ThrowsInvalidDeliveryStatusTransitionException()
    {
        var delivery = CreateDelivery();
        delivery.Assign(Guid.NewGuid());
        var act = () => delivery.Assign(Guid.NewGuid());
        act.Should().Throw<InvalidDeliveryStatusTransitionException>();
    }

    [Fact]
    public void PickUp_WhenAssigned_SetsStatusInProgress()
    {
        var delivery = CreateDelivery();
        delivery.Assign(Guid.NewGuid());
        delivery.PickUp();
        delivery.Status.Should().Be(DeliveryStatus.InProgress);
    }

    [Fact]
    public void PickUp_WhenPending_ThrowsInvalidDeliveryStatusTransitionException()
    {
        var delivery = CreateDelivery();
        var act = () => delivery.PickUp();
        act.Should().Throw<InvalidDeliveryStatusTransitionException>();
    }

    [Fact]
    public void PickUp_RaisesDeliveryPickedUpDomainEvent()
    {
        var delivery = CreateDelivery();
        delivery.Assign(Guid.NewGuid());
        delivery.ClearDomainEvents();
        delivery.PickUp();
        delivery.DomainEvents.Should().ContainSingle(e => e is DeliveryPickedUpDomainEvent);
    }

    [Fact]
    public void Complete_WhenInProgress_SetsStatusDelivered()
    {
        var delivery = CreateDelivery();
        delivery.Assign(Guid.NewGuid());
        delivery.PickUp();
        delivery.Complete();
        delivery.Status.Should().Be(DeliveryStatus.Delivered);
    }

    [Fact]
    public void Complete_WhenAssigned_ThrowsInvalidDeliveryStatusTransitionException()
    {
        var delivery = CreateDelivery();
        delivery.Assign(Guid.NewGuid());
        var act = () => delivery.Complete();
        act.Should().Throw<InvalidDeliveryStatusTransitionException>();
    }

    [Fact]
    public void Complete_RaisesDeliveryCompletedDomainEvent()
    {
        var delivery = CreateDelivery();
        delivery.Assign(Guid.NewGuid());
        delivery.PickUp();
        delivery.ClearDomainEvents();
        delivery.Complete();
        delivery.DomainEvents.Should().ContainSingle(e => e is DeliveryCompletedDomainEvent);
    }

    [Fact]
    public void Fail_WhenDelivered_ThrowsInvalidDeliveryStatusTransitionException()
    {
        var delivery = CreateDelivery();
        delivery.Assign(Guid.NewGuid());
        delivery.PickUp();
        delivery.Complete();
        var act = () => delivery.Fail("reason");
        act.Should().Throw<InvalidDeliveryStatusTransitionException>();
    }
}
