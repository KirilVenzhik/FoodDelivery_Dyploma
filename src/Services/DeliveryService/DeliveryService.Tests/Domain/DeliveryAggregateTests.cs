using DeliveryService.Domain.Entities;
using DeliveryService.Domain.Enums;
using DeliveryService.Domain.Events;
using DeliveryService.Domain.Exceptions;
using FluentAssertions;

namespace DeliveryService.Tests.Domain;

public class DeliveryAggregateTests
{
    private static Delivery MakeDelivery()
        => Delivery.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

    [Fact]
    public void Create_SetsStatusPending()
    {
        var d = MakeDelivery();
        d.Status.Should().Be(DeliveryStatus.Pending);
    }

    [Fact]
    public void Create_RaisesDeliveryCreatedDomainEvent()
    {
        var d = MakeDelivery();
        d.DomainEvents.Should().ContainSingle(e => e is DeliveryCreatedDomainEvent);
    }

    [Fact]
    public void Assign_WhenPending_SetsStatusAssigned()
    {
        var d = MakeDelivery();
        d.Assign(Guid.NewGuid());
        d.Status.Should().Be(DeliveryStatus.Assigned);
    }

    [Fact]
    public void Assign_WhenPending_SetsCourierId()
    {
        var d = MakeDelivery();
        var courierId = Guid.NewGuid();
        d.Assign(courierId);
        d.CourierId.Should().Be(courierId);
    }

    [Fact]
    public void Assign_RaisesDeliveryAssignedDomainEvent()
    {
        var d = MakeDelivery();
        d.ClearDomainEvents();
        d.Assign(Guid.NewGuid());
        d.DomainEvents.Should().ContainSingle(e => e is DeliveryAssignedDomainEvent);
    }

    [Fact]
    public void Assign_WhenAlreadyAssigned_ThrowsInvalidDeliveryStatusTransitionException()
    {
        var d = MakeDelivery();
        d.Assign(Guid.NewGuid());
        var act = () => d.Assign(Guid.NewGuid());
        act.Should().Throw<InvalidDeliveryStatusTransitionException>();
    }

    [Fact]
    public void PickUp_WhenAssigned_SetsStatusInProgress()
    {
        var d = MakeDelivery();
        d.Assign(Guid.NewGuid());
        d.PickUp();
        d.Status.Should().Be(DeliveryStatus.InProgress);
    }

    [Fact]
    public void PickUp_WhenPending_ThrowsInvalidDeliveryStatusTransitionException()
    {
        var d = MakeDelivery();
        var act = () => d.PickUp();
        act.Should().Throw<InvalidDeliveryStatusTransitionException>();
    }

    [Fact]
    public void PickUp_RaisesDeliveryPickedUpDomainEvent()
    {
        var d = MakeDelivery();
        d.Assign(Guid.NewGuid());
        d.ClearDomainEvents();
        d.PickUp();
        d.DomainEvents.Should().ContainSingle(e => e is DeliveryPickedUpDomainEvent);
    }

    [Fact]
    public void Complete_WhenInProgress_SetsStatusDelivered()
    {
        var d = MakeDelivery();
        d.Assign(Guid.NewGuid());
        d.PickUp();
        d.Complete();
        d.Status.Should().Be(DeliveryStatus.Delivered);
    }

    [Fact]
    public void Complete_WhenAssigned_ThrowsInvalidDeliveryStatusTransitionException()
    {
        var d = MakeDelivery();
        d.Assign(Guid.NewGuid());
        var act = () => d.Complete();
        act.Should().Throw<InvalidDeliveryStatusTransitionException>();
    }

    [Fact]
    public void Complete_RaisesDeliveryCompletedDomainEvent()
    {
        var d = MakeDelivery();
        d.Assign(Guid.NewGuid());
        d.PickUp();
        d.ClearDomainEvents();
        d.Complete();
        d.DomainEvents.Should().ContainSingle(e => e is DeliveryCompletedDomainEvent);
    }

    [Fact]
    public void Fail_WhenDelivered_ThrowsInvalidDeliveryStatusTransitionException()
    {
        var d = MakeDelivery();
        d.Assign(Guid.NewGuid());
        d.PickUp();
        d.Complete();
        var act = () => d.Fail("reason");
        act.Should().Throw<InvalidDeliveryStatusTransitionException>();
    }
}
