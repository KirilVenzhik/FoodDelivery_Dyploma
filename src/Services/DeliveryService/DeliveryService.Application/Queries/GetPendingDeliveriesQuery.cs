using DeliveryService.Application.DTOs;
using DeliveryService.Application.Interfaces;

namespace DeliveryService.Application.Queries;

public record GetPendingDeliveriesQuery();

public sealed class GetPendingDeliveriesQueryHandler
{
    private readonly IDeliveryRepository _repository;

    public GetPendingDeliveriesQueryHandler(IDeliveryRepository repository)
        => _repository = repository;

    public async Task<List<DeliveryResponse>> HandleAsync(GetPendingDeliveriesQuery query, CancellationToken ct = default)
    {
        var deliveries = await _repository.GetPendingAsync(ct);
        return deliveries.Select(DeliveryResponse.FromDomain).ToList();
    }
}
