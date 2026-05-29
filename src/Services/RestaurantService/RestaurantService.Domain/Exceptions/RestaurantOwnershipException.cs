namespace RestaurantService.Domain.Exceptions;

public sealed class RestaurantOwnershipException : Exception
{
    public RestaurantOwnershipException(Guid restaurantId, Guid requestingOwnerId)
        : base($"User '{requestingOwnerId}' does not own restaurant '{restaurantId}'.") { }
}
