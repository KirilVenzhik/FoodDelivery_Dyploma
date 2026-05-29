using FluentValidation;
using RestaurantService.Application.DTOs;
using RestaurantService.Application.Interfaces;
using RestaurantService.Domain.Exceptions;

namespace RestaurantService.Application.Commands;

public record AddMenuItemCommand(
    Guid RestaurantId,
    Guid RequestingOwnerId,
    string Name,
    string Description,
    decimal Price);

public sealed class AddMenuItemCommandHandler
{
    private readonly IRestaurantRepository _repository;
    private readonly IValidator<AddMenuItemCommand> _validator;

    public AddMenuItemCommandHandler(
        IRestaurantRepository repository,
        IValidator<AddMenuItemCommand> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<MenuItemResponse> HandleAsync(
        AddMenuItemCommand command,
        CancellationToken ct = default)
    {
        await _validator.ValidateAndThrowAsync(command, ct);

        var restaurant = await _repository.GetByIdWithItemsAsync(command.RestaurantId, ct)
            ?? throw new RestaurantNotFoundException(command.RestaurantId);

        if (restaurant.OwnerId != command.RequestingOwnerId)
            throw new RestaurantOwnershipException(command.RestaurantId, command.RequestingOwnerId);

        var menuItem = restaurant.AddMenuItem(command.Name, command.Description, command.Price);
        await _repository.SaveChangesAsync(ct);

        return MenuItemResponse.FromDomain(menuItem);
    }
}
