using DeliveryService.Application.DTOs;
using DeliveryService.Application.Interfaces;

namespace DeliveryService.Application.Queries;

public record GetMyDeliveriesQuery(Guid CourierId);

public sealed class GetMyDeliveriesQueryHandler
{
    private readonly IDeliveryRepository _repository;

    public GetMyDeliveriesQueryHandler(IDeliveryRepository repository)
        => _repository = repository;

    public async Task<List<DeliveryResponse>> HandleAsync(GetMyDeliveriesQuery query, CancellationToken ct = default)
    {
        var deliveries = await _repository.GetByCourierIdAsync(query.CourierId, ct);
        return deliveries.Select(DeliveryResponse.FromDomain).ToList();
    }
}
