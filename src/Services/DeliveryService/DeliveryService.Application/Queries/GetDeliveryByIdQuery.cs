using DeliveryService.Application.Common;
using DeliveryService.Application.DTOs;
using DeliveryService.Application.Interfaces;

namespace DeliveryService.Application.Queries;

public record GetDeliveryByIdQuery(Guid DeliveryId);

public sealed class GetDeliveryByIdQueryHandler
{
    private readonly IDeliveryRepository _repository;

    public GetDeliveryByIdQueryHandler(IDeliveryRepository repository)
        => _repository = repository;

    public async Task<QueryResult<DeliveryResponse>> HandleAsync(
        GetDeliveryByIdQuery query,
        CancellationToken ct = default)
    {
        var delivery = await _repository.GetByIdAsync(query.DeliveryId, ct);
        return delivery is null
            ? QueryResult<DeliveryResponse>.NotFound()
            : QueryResult<DeliveryResponse>.Ok(DeliveryResponse.FromDomain(delivery));
    }
}
