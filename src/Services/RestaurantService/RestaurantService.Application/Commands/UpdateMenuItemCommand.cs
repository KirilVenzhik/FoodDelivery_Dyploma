using FluentValidation;
using RestaurantService.Application.DTOs;
using RestaurantService.Application.Interfaces;
using RestaurantService.Domain.Exceptions;

namespace RestaurantService.Application.Commands;

public record UpdateMenuItemCommand(
    Guid RestaurantId,
    Guid MenuItemId,
    Guid RequestingOwnerId,
    string? Name,
    string? Description,
    decimal? Price,
    bool? Available);

public sealed class UpdateMenuItemCommandHandler
{
    private readonly IRestaurantRepository _repository;
    private readonly IValidator<UpdateMenuItemCommand> _validator;

    public UpdateMenuItemCommandHandler(
        IRestaurantRepository repository,
        IValidator<UpdateMenuItemCommand> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<MenuItemResponse> HandleAsync(
        UpdateMenuItemCommand command,
        CancellationToken ct = default)
    {
        await _validator.ValidateAndThrowAsync(command, ct);

        var restaurant = await _repository.GetByIdWithItemsAsync(command.RestaurantId, ct)
            ?? throw new RestaurantNotFoundException(command.RestaurantId);

        if (restaurant.OwnerId != command.RequestingOwnerId)
            throw new RestaurantOwnershipException(command.RestaurantId, command.RequestingOwnerId);

        restaurant.UpdateMenuItem(
            command.MenuItemId,
            command.Name,
            command.Description,
            command.Price,
            command.Available);

        await _repository.SaveChangesAsync(ct);

        var item = restaurant.MenuItems.First(m => m.Id == command.MenuItemId);
        return MenuItemResponse.FromDomain(item);
    }
}
