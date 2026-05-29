using FluentValidation;
using RestaurantService.Application.DTOs;
using RestaurantService.Application.Interfaces;
using RestaurantService.Domain.Entities;

namespace RestaurantService.Application.Commands;

public record CreateRestaurantCommand(
    Guid OwnerId,
    string Name,
    string Description,
    string Address);

public sealed class CreateRestaurantCommandHandler
{
    private readonly IRestaurantRepository _repository;
    private readonly IValidator<CreateRestaurantCommand> _validator;

    public CreateRestaurantCommandHandler(
        IRestaurantRepository repository,
        IValidator<CreateRestaurantCommand> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<RestaurantResponse> HandleAsync(
        CreateRestaurantCommand command,
        CancellationToken ct = default)
    {
        await _validator.ValidateAndThrowAsync(command, ct);

        var restaurant = Restaurant.Create(
            command.OwnerId,
            command.Name,
            command.Description,
            command.Address);

        await _repository.AddAsync(restaurant, ct);
        await _repository.SaveChangesAsync(ct);

        return RestaurantResponse.FromDomain(restaurant);
    }
}
