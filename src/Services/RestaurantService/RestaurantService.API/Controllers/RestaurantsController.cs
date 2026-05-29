using Auth.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantService.Application.Commands;
using RestaurantService.Application.DTOs;
using RestaurantService.Application.Queries;

namespace RestaurantService.API.Controllers;

[ApiController]
[Route("api/restaurants")]
public class RestaurantsController : ControllerBase
{
    private readonly CreateRestaurantCommandHandler _createHandler;
    private readonly UpdateRestaurantCommandHandler _updateHandler;
    private readonly ChangeRestaurantAvailabilityCommandHandler _availabilityHandler;
    private readonly AddMenuItemCommandHandler _addMenuItemHandler;
    private readonly UpdateMenuItemCommandHandler _updateMenuItemHandler;
    private readonly RemoveMenuItemCommandHandler _removeMenuItemHandler;
    private readonly GetAllRestaurantsQueryHandler _getAllHandler;
    private readonly GetRestaurantByIdQueryHandler _getByIdHandler;
    private readonly GetMyRestaurantsQueryHandler _getMyHandler;
    private readonly ICurrentUser _currentUser;

    public RestaurantsController(
        CreateRestaurantCommandHandler createHandler,
        UpdateRestaurantCommandHandler updateHandler,
        ChangeRestaurantAvailabilityCommandHandler availabilityHandler,
        AddMenuItemCommandHandler addMenuItemHandler,
        UpdateMenuItemCommandHandler updateMenuItemHandler,
        RemoveMenuItemCommandHandler removeMenuItemHandler,
        GetAllRestaurantsQueryHandler getAllHandler,
        GetRestaurantByIdQueryHandler getByIdHandler,
        GetMyRestaurantsQueryHandler getMyHandler,
        ICurrentUser currentUser)
    {
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _availabilityHandler = availabilityHandler;
        _addMenuItemHandler = addMenuItemHandler;
        _updateMenuItemHandler = updateMenuItemHandler;
        _removeMenuItemHandler = removeMenuItemHandler;
        _getAllHandler = getAllHandler;
        _getByIdHandler = getByIdHandler;
        _getMyHandler = getMyHandler;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _getAllHandler.HandleAsync(new GetAllRestaurantsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getByIdHandler.HandleAsync(
            new GetRestaurantByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "RestaurantOwner")]
    [HttpGet("my")]
    public async Task<IActionResult> GetMy(CancellationToken cancellationToken)
    {
        var result = await _getMyHandler.HandleAsync(
            new GetMyRestaurantsQuery(_currentUser.UserId), cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "RestaurantOwner")]
    [HttpPost]
    public async Task<IActionResult> Create(
        CreateRestaurantRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createHandler.HandleAsync(
            new CreateRestaurantCommand(
                _currentUser.UserId,
                request.Name,
                request.Description,
                request.Address),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = "RestaurantOwner")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateRestaurantRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _updateHandler.HandleAsync(
            new UpdateRestaurantCommand(
                id,
                _currentUser.UserId,
                request.Name,
                request.Description,
                request.Address),
            cancellationToken);

        return Ok(result);
    }

    [Authorize(Roles = "RestaurantOwner")]
    [HttpPost("{id:guid}/open")]
    public async Task<IActionResult> Open(Guid id, CancellationToken cancellationToken)
    {
        var result = await _availabilityHandler.HandleAsync(
            new ChangeRestaurantAvailabilityCommand(id, _currentUser.UserId, true),
            cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "RestaurantOwner")]
    [HttpPost("{id:guid}/close")]
    public async Task<IActionResult> Close(Guid id, CancellationToken cancellationToken)
    {
        var result = await _availabilityHandler.HandleAsync(
            new ChangeRestaurantAvailabilityCommand(id, _currentUser.UserId, false),
            cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "RestaurantOwner")]
    [HttpPost("{id:guid}/menu-items")]
    public async Task<IActionResult> AddMenuItem(
        Guid id,
        AddMenuItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _addMenuItemHandler.HandleAsync(
            new AddMenuItemCommand(
                id,
                _currentUser.UserId,
                request.Name,
                request.Description,
                request.Price),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id }, result);
    }

    [Authorize(Roles = "RestaurantOwner")]
    [HttpPut("{id:guid}/menu-items/{itemId:guid}")]
    public async Task<IActionResult> UpdateMenuItem(
        Guid id,
        Guid itemId,
        UpdateMenuItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _updateMenuItemHandler.HandleAsync(
            new UpdateMenuItemCommand(
                id,
                itemId,
                _currentUser.UserId,
                request.Name,
                request.Description,
                request.Price,
                request.Available),
            cancellationToken);

        return Ok(result);
    }

    [Authorize(Roles = "RestaurantOwner")]
    [HttpDelete("{id:guid}/menu-items/{itemId:guid}")]
    public async Task<IActionResult> RemoveMenuItem(
        Guid id,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        await _removeMenuItemHandler.HandleAsync(
            new RemoveMenuItemCommand(id, itemId, _currentUser.UserId),
            cancellationToken);

        return NoContent();
    }
}
