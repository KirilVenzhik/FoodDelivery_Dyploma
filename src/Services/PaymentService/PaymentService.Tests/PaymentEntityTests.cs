using FluentAssertions;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using PaymentService.Domain.Events;
using PaymentService.Domain.Exceptions;

namespace PaymentService.Tests;

public class PaymentEntityTests
{
    private static Payment CreatePendingPayment() =>
        Payment.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 100m);

    [Fact]
    public void Create_WithValidData_SetsStatusPending()
    {
        var payment = CreatePendingPayment();

        payment.Status.Should().Be(PaymentStatus.Pending);
    }

    [Fact]
    public void Create_RaisesPaymentCreatedDomainEvent()
    {
        var payment = CreatePendingPayment();

        payment.DomainEvents.Should().ContainSingle(e => e is PaymentCreatedDomainEvent);
    }

    [Fact]
    public void Complete_SetsStatusCompleted()
    {
        var payment = CreatePendingPayment();

        payment.Complete();

        payment.Status.Should().Be(PaymentStatus.Completed);
    }

    [Fact]
    public void Complete_SetsProcessedAt()
    {
        var before = DateTime.UtcNow;
        var payment = CreatePendingPayment();

        payment.Complete();

        payment.ProcessedAt.Should().NotBeNull();
        payment.ProcessedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void Complete_RaisesPaymentCompletedDomainEvent()
    {
        var payment = CreatePendingPayment();
        payment.ClearDomainEvents();

        payment.Complete();

        payment.DomainEvents.Should().ContainSingle(e => e is PaymentCompletedDomainEvent);
    }

    [Fact]
    public void Fail_SetsStatusFailed()
    {
        var payment = CreatePendingPayment();

        payment.Fail("Insufficient funds.");

        payment.Status.Should().Be(PaymentStatus.Failed);
    }

    [Fact]
    public void Fail_RaisesPaymentFailedDomainEvent()
    {
        var payment = CreatePendingPayment();
        payment.ClearDomainEvents();

        payment.Fail("Insufficient funds.");

        payment.DomainEvents.Should().ContainSingle(e => e is PaymentFailedDomainEvent);
    }

    [Fact]
    public void Complete_WhenAlreadyCompleted_ThrowsInvalidPaymentOperationException()
    {
        var payment = CreatePendingPayment();
        payment.Complete();

        var act = () => payment.Complete();

        act.Should().Throw<InvalidPaymentOperationException>();
    }

    [Fact]
    public void Fail_WhenAlreadyCompleted_ThrowsInvalidPaymentOperationException()
    {
        var payment = CreatePendingPayment();
        payment.Complete();

        var act = () => payment.Fail("reason");

        act.Should().Throw<InvalidPaymentOperationException>();
    }
}
