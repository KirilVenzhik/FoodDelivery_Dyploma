using DeliveryService.Application.DTOs;
using DeliveryService.Application.Interfaces;
using DeliveryService.Domain.Exceptions;

namespace DeliveryService.Application.Queries;

public record GetDeliveryByIdQuery(Guid DeliveryId, Guid RequestingUserId, string RequestingRole);

public sealed class GetDeliveryByIdQueryHandler
{
    private readonly IDeliveryRepository _repository;

    public GetDeliveryByIdQueryHandler(IDeliveryRepository repository)
        => _repository = repository;

    public async Task<DeliveryResponse> HandleAsync(GetDeliveryByIdQuery query, CancellationToken ct = default)
    {
        var delivery = await _repository.GetByIdAsync(query.DeliveryId, ct)
            ?? throw new DeliveryNotFoundException(query.DeliveryId);

        if (query.RequestingRole == "Customer" && delivery.ClientId != query.RequestingUserId)
            throw new UnauthorizedAccessException("You are not authorized to view this delivery.");

        if (query.RequestingRole == "Courier" && delivery.CourierId != query.RequestingUserId)
            throw new UnauthorizedAccessException("You are not authorized to view this delivery.");

        return DeliveryResponse.FromDomain(delivery);
    }
}
