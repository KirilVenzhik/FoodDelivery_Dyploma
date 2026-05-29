using FluentAssertions;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using PaymentService.Domain.Events;

namespace PaymentService.Tests.Domain;

public class PaymentEntityTests
{
    private static Payment MakePayment()
        => Payment.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 99.99m);

    [Fact]
    public void Create_WithValidData_SetsStatusPending()
    {
        var payment = MakePayment();

        payment.Status.Should().Be(PaymentStatus.Pending);
        payment.ProcessedAt.Should().BeNull();
    }

    [Fact]
    public void Create_RaisesPaymentCreatedDomainEvent()
    {
        var payment = MakePayment();

        payment.DomainEvents.Should().ContainSingle(e => e is PaymentCreatedDomainEvent);
    }

    [Fact]
    public void Complete_SetsStatusCompleted()
    {
        var payment = MakePayment();

        payment.Complete();

        payment.Status.Should().Be(PaymentStatus.Completed);
    }

    [Fact]
    public void Complete_RaisesPaymentCompletedDomainEvent()
    {
        var payment = MakePayment();
        payment.ClearDomainEvents();

        payment.Complete();

        payment.DomainEvents.Should().ContainSingle(e => e is PaymentCompletedDomainEvent);
    }

    [Fact]
    public void Fail_SetsStatusFailed()
    {
        var payment = MakePayment();

        payment.Fail("Insufficient funds");

        payment.Status.Should().Be(PaymentStatus.Failed);
    }
}
