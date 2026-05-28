using Auth.Abstractions;
using AuthService.Application.Commands;
using AuthService.Application.DTOs;
using AuthService.Application.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IRegisterUserCommandHandler _registerHandler;
    private readonly ILoginUserCommandHandler _loginHandler;
    private readonly IRefreshTokenCommandHandler _refreshHandler;
    private readonly IGetCurrentUserQueryHandler _getCurrentUserHandler;

    public AuthController(
        IRegisterUserCommandHandler registerHandler,
        ILoginUserCommandHandler loginHandler,
        IRefreshTokenCommandHandler refreshHandler,
        IGetCurrentUserQueryHandler getCurrentUserHandler)
    {
        _registerHandler = registerHandler;
        _loginHandler = loginHandler;
        _refreshHandler = refreshHandler;
        _getCurrentUserHandler = getCurrentUserHandler;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _registerHandler.HandleAsync(
            new RegisterUserCommand(request.Email, request.Password, request.Role),
            cancellationToken);

        return CreatedAtAction(nameof(Register), response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _loginHandler.HandleAsync(
            new LoginUserCommand(request.Email, request.Password),
            cancellationToken);

        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _refreshHandler.HandleAsync(
            new RefreshTokenCommand(request.RefreshToken),
            cancellationToken);

        return Ok(response);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me(
        [FromServices] ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        var result = await _getCurrentUserHandler.HandleAsync(
            new GetCurrentUserQuery(currentUser.UserId),
            cancellationToken);

        return Ok(result);
    }
}
