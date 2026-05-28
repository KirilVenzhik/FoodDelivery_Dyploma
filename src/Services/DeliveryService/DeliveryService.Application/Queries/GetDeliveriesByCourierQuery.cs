using DeliveryService.Application.DTOs;
using DeliveryService.Application.Interfaces;

namespace DeliveryService.Application.Queries;

public record GetDeliveriesByCourierQuery(Guid CourierId);

public sealed class GetDeliveriesByCourierQueryHandler
{
    private readonly IDeliveryRepository _repository;

    public GetDeliveriesByCourierQueryHandler(IDeliveryRepository repository)
        => _repository = repository;

    public async Task<List<DeliveryResponse>> HandleAsync(
        GetDeliveriesByCourierQuery query,
        CancellationToken ct = default)
    {
        var deliveries = await _repository.GetByCourierIdAsync(query.CourierId, ct);
        return deliveries.Select(DeliveryResponse.FromDomain).ToList();
    }
}
