using PaymentService.Domain.Entities;

namespace PaymentService.Application.Interfaces;

public interface IPaymentRepository
{
    Task AddAsync(Payment payment, CancellationToken cancellationToken = default);

    Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
