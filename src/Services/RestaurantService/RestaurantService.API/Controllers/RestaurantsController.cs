using Auth.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantService.Application.Commands;
using RestaurantService.Application.Common;
using RestaurantService.Application.DTOs;
using RestaurantService.Application.Queries;

namespace RestaurantService.API.Controllers;

[ApiController]
[Route("api/restaurants")]
public class RestaurantsController : ControllerBase
{
    private readonly CreateRestaurantCommandHandler _createHandler;
    private readonly AddMenuItemCommandHandler _addMenuItemHandler;
    private readonly UpdateMenuItemCommandHandler _updateMenuItemHandler;
    private readonly DeleteMenuItemCommandHandler _deleteMenuItemHandler;
    private readonly OpenRestaurantCommandHandler _openHandler;
    private readonly CloseRestaurantCommandHandler _closeHandler;
    private readonly GetAllRestaurantsQueryHandler _getAllHandler;
    private readonly GetRestaurantByIdQueryHandler _getByIdHandler;
    private readonly GetMenuItemsQueryHandler _getMenuItemsHandler;
    private readonly ICurrentUser _currentUser;

    public RestaurantsController(
        CreateRestaurantCommandHandler createHandler,
        AddMenuItemCommandHandler addMenuItemHandler,
        UpdateMenuItemCommandHandler updateMenuItemHandler,
        DeleteMenuItemCommandHandler deleteMenuItemHandler,
        OpenRestaurantCommandHandler openHandler,
        CloseRestaurantCommandHandler closeHandler,
        GetAllRestaurantsQueryHandler getAllHandler,
        GetRestaurantByIdQueryHandler getByIdHandler,
        GetMenuItemsQueryHandler getMenuItemsHandler,
        ICurrentUser currentUser)
    {
        _createHandler = createHandler;
        _addMenuItemHandler = addMenuItemHandler;
        _updateMenuItemHandler = updateMenuItemHandler;
        _deleteMenuItemHandler = deleteMenuItemHandler;
        _openHandler = openHandler;
        _closeHandler = closeHandler;
        _getAllHandler = getAllHandler;
        _getByIdHandler = getByIdHandler;
        _getMenuItemsHandler = getMenuItemsHandler;
        _currentUser = currentUser;
    }

    [Authorize(Roles = "RestaurantManager")]
    [HttpPost]
    public async Task<IActionResult> Create(
        CreateRestaurantRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createHandler.HandleAsync(
            new CreateRestaurantCommand(_currentUser.UserId, request.Name), cancellationToken);

        return result.Status switch
        {
            CommandResultStatus.Success => Ok(result.CreatedId),
            CommandResultStatus.Conflict => Conflict("Restaurant already exists."),
            _ => StatusCode(500)
        };
    }

    [Authorize(Roles = "RestaurantManager")]
    [HttpPost("{restaurantId:guid}/menu-items")]
    public async Task<IActionResult> AddMenuItem(
        Guid restaurantId,
        AddMenuItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _addMenuItemHandler.HandleAsync(
            new AddMenuItemCommand(restaurantId, _currentUser.UserId, request.Name, request.Price),
            cancellationToken);

        return result.Status switch
        {
            CommandResultStatus.Success => Ok(result.CreatedId),
            CommandResultStatus.NotFound => NotFound(),
            CommandResultStatus.Forbidden => Forbid(),
            _ => StatusCode(500)
        };
    }

    [Authorize(Roles = "RestaurantManager")]
    [HttpPut("{restaurantId:guid}/menu-items/{menuItemId:guid}")]
    public async Task<IActionResult> UpdateMenuItem(
        Guid restaurantId,
        Guid menuItemId,
        UpdateMenuItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _updateMenuItemHandler.HandleAsync(
            new UpdateMenuItemCommand(restaurantId, menuItemId, _currentUser.UserId, request.Name, request.Price),
            cancellationToken);

        return result.Status switch
        {
            CommandResultStatus.Success => NoContent(),
            CommandResultStatus.NotFound => NotFound(),
            CommandResultStatus.Forbidden => Forbid(),
            _ => StatusCode(500)
        };
    }

    [Authorize(Roles = "RestaurantManager")]
    [HttpDelete("{restaurantId:guid}/menu-items/{menuItemId:guid}")]
    public async Task<IActionResult> DeleteMenuItem(
        Guid restaurantId,
        Guid menuItemId,
        CancellationToken cancellationToken)
    {
        var result = await _deleteMenuItemHandler.HandleAsync(
            new DeleteMenuItemCommand(restaurantId, menuItemId, _currentUser.UserId),
            cancellationToken);

        return result.Status switch
        {
            CommandResultStatus.Success => NoContent(),
            CommandResultStatus.NotFound => NotFound(),
            CommandResultStatus.Forbidden => Forbid(),
            _ => StatusCode(500)
        };
    }

    [Authorize(Roles = "RestaurantManager")]
    [HttpPost("{restaurantId:guid}/open")]
    public async Task<IActionResult> Open(
        Guid restaurantId,
        CancellationToken cancellationToken)
    {
        var result = await _openHandler.HandleAsync(
            new OpenRestaurantCommand(restaurantId, _currentUser.UserId), cancellationToken);

        return result.Status switch
        {
            CommandResultStatus.Success => Ok(),
            CommandResultStatus.NotFound => NotFound(),
            CommandResultStatus.Forbidden => Forbid(),
            _ => StatusCode(500)
        };
    }

    [Authorize(Roles = "RestaurantManager")]
    [HttpPost("{restaurantId:guid}/close")]
    public async Task<IActionResult> Close(
        Guid restaurantId,
        CancellationToken cancellationToken)
    {
        var result = await _closeHandler.HandleAsync(
            new CloseRestaurantCommand(restaurantId, _currentUser.UserId), cancellationToken);

        return result.Status switch
        {
            CommandResultStatus.Success => Ok(),
            CommandResultStatus.NotFound => NotFound(),
            CommandResultStatus.Forbidden => Forbid(),
            _ => StatusCode(500)
        };
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var restaurants = await _getAllHandler.HandleAsync(cancellationToken);
        return Ok(restaurants);
    }

    [Authorize]
    [HttpGet("{restaurantId:guid}")]
    public async Task<IActionResult> GetById(
        Guid restaurantId,
        CancellationToken cancellationToken)
    {
        var restaurant = await _getByIdHandler.HandleAsync(
            new GetRestaurantByIdQuery(restaurantId), cancellationToken);

        return restaurant is null ? NotFound() : Ok(restaurant);
    }

    [Authorize]
    [HttpGet("{restaurantId:guid}/menu-items")]
    public async Task<IActionResult> GetMenuItems(
        Guid restaurantId,
        CancellationToken cancellationToken)
    {
        var items = await _getMenuItemsHandler.HandleAsync(
            new GetMenuItemsQuery(restaurantId), cancellationToken);

        return items is null ? NotFound() : Ok(items);
    }
}
