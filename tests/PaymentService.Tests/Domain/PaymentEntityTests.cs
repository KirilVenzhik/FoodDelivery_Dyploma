using Xunit;
using FluentAssertions;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;

namespace PaymentService.Tests.Domain;

public class PaymentEntityTests
{
    [Fact]
    public void Create_SetsPendingStatus()
    {
        var payment = Payment.Create(Guid.NewGuid(), Guid.NewGuid(), 100m);
        payment.Status.Should().Be(PaymentStatus.Pending);
    }

    [Fact]
    public void Create_SetsOrderId()
    {
        var orderId = Guid.NewGuid();
        var payment = Payment.Create(orderId, Guid.NewGuid(), 100m);
        payment.OrderId.Should().Be(orderId);
    }

    [Fact]
    public void Create_SetsCorrelationId()
    {
        var correlationId = Guid.NewGuid();
        var payment = Payment.Create(Guid.NewGuid(), correlationId, 100m);
        payment.CorrelationId.Should().Be(correlationId);
    }

    [Fact]
    public void Create_SetsAmount()
    {
        var payment = Payment.Create(Guid.NewGuid(), Guid.NewGuid(), 99.99m);
        payment.Amount.Should().Be(99.99m);
    }

    [Fact]
    public void Create_SetsCreatedAt()
    {
        var before = DateTime.UtcNow;
        var payment = Payment.Create(Guid.NewGuid(), Guid.NewGuid(), 100m);
        var after = DateTime.UtcNow;
        payment.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Create_ProcessedAtIsNull()
    {
        var payment = Payment.Create(Guid.NewGuid(), Guid.NewGuid(), 100m);
        payment.ProcessedAt.Should().BeNull();
    }

    [Fact]
    public void Complete_SetsCompletedStatus()
    {
        var payment = Payment.Create(Guid.NewGuid(), Guid.NewGuid(), 100m);
        payment.Complete();
        payment.Status.Should().Be(PaymentStatus.Completed);
    }

    [Fact]
    public void Complete_SetsProcessedAt()
    {
        var payment = Payment.Create(Guid.NewGuid(), Guid.NewGuid(), 100m);
        var before = DateTime.UtcNow;
        payment.Complete();
        payment.ProcessedAt.Should().NotBeNull().And.BeOnOrAfter(before);
    }

    [Fact]
    public void Fail_SetsFailedStatus()
    {
        var payment = Payment.Create(Guid.NewGuid(), Guid.NewGuid(), 100m);
        payment.Fail();
        payment.Status.Should().Be(PaymentStatus.Failed);
    }

    [Fact]
    public void Fail_SetsProcessedAt()
    {
        var payment = Payment.Create(Guid.NewGuid(), Guid.NewGuid(), 100m);
        var before = DateTime.UtcNow;
        payment.Fail();
        payment.ProcessedAt.Should().NotBeNull().And.BeOnOrAfter(before);
    }
}
