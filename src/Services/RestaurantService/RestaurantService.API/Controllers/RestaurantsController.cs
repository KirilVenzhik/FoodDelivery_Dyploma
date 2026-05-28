using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantService.Application.DTOs;
using RestaurantService.Application.Interfaces;
using RestaurantService.Domain.Entities;
using Auth.Abstractions;

namespace RestaurantService.API.Controllers;

[ApiController]
[Route("api/restaurants")]
public class RestaurantsController : ControllerBase
{
    private readonly IRestaurantRepository _repository;
    private readonly ICurrentUser _currentUser;

    public RestaurantsController(
        IRestaurantRepository repository,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    [Authorize(Roles = "RestaurantOwner")]
    [HttpPost]
    public async Task<IActionResult> Create(
        CreateRestaurantRequest request,
        CancellationToken cancellationToken)
    {
        var existingRestaurant = await _repository
            .GetByOwnerIdAsync(
                _currentUser.UserId,
                cancellationToken);

        if (existingRestaurant is not null)
        {
            return Conflict(
                "Restaurant already exists.");
        }

        var restaurant = Restaurant.Create(
            _currentUser.UserId,
            request.Name);

        await _repository.AddAsync(
            restaurant,
            cancellationToken);

        await _repository.SaveChangesAsync(
            cancellationToken);

        return Ok(restaurant.Id);
    }

    [Authorize(Roles = "RestaurantOwner")]
    [HttpPost("{restaurantId:guid}/menu-items")]
    public async Task<IActionResult> AddMenuItem(
        Guid restaurantId,
        AddMenuItemRequest request,
        CancellationToken cancellationToken)
    {
        var restaurant = await _repository
            .GetByIdAsync(
                restaurantId,
                cancellationToken);

        if (restaurant is null)
        {
            return NotFound();
        }

        if (restaurant.OwnerUserId != _currentUser.UserId)
        {
            return Forbid();
        }

        restaurant.AddMenuItem(
            request.Name,
            request.Price);

        await _repository.SaveChangesAsync(
            cancellationToken);

        return Ok();
    }

    [Authorize(Roles = "RestaurantOwner")]
    [HttpPost("{restaurantId:guid}/open")]
    public async Task<IActionResult> Open(
        Guid restaurantId,
        CancellationToken cancellationToken)
    {
        var restaurant = await _repository
            .GetByIdAsync(
                restaurantId,
                cancellationToken);

        if (restaurant is null)
        {
            return NotFound();
        }

        if (restaurant.OwnerUserId != _currentUser.UserId)
        {
            return Forbid();
        }

        restaurant.Open();

        await _repository.SaveChangesAsync(
            cancellationToken);

        return Ok();
    }

    [Authorize(Roles = "RestaurantOwner")]
    [HttpPost("{restaurantId:guid}/close")]
    public async Task<IActionResult> Close(
        Guid restaurantId,
        CancellationToken cancellationToken)
    {
        var restaurant = await _repository
            .GetByIdAsync(
                restaurantId,
                cancellationToken);

        if (restaurant is null)
        {
            return NotFound();
        }

        if (restaurant.OwnerUserId != _currentUser.UserId)
        {
            return Forbid();
        }

        restaurant.Close();

        await _repository.SaveChangesAsync(
            cancellationToken);

        return Ok();
    }
}