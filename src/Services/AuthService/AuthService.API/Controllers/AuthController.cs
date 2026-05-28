using Auth.Abstractions;
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;

    public AuthController(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var exists = await _userRepository
            .ExistsByEmailAsync(request.Email, cancellationToken);

        if (exists)
            return Conflict("Email already exists.");

        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = Domain.Entities.User.Create(request.Email, passwordHash, request.Role);

        var refreshToken = _jwtProvider.GenerateRefreshToken();
        user.SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return Ok(new AuthResponse
        {
            AccessToken = _jwtProvider.Generate(user),
            RefreshToken = refreshToken
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository
            .GetByEmailAsync(request.Email, cancellationToken);

        if (user is null)
            return Unauthorized();

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            return Unauthorized();

        var refreshToken = _jwtProvider.GenerateRefreshToken();
        user.SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));
        await _userRepository.SaveChangesAsync(cancellationToken);

        return Ok(new AuthResponse
        {
            AccessToken = _jwtProvider.Generate(user),
            RefreshToken = refreshToken
        });
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository
            .GetByRefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (user is null || user.RefreshTokenExpiry < DateTime.UtcNow)
            return Unauthorized("Invalid or expired refresh token.");

        var newRefreshToken = _jwtProvider.GenerateRefreshToken();
        user.SetRefreshToken(newRefreshToken, DateTime.UtcNow.AddDays(7));
        await _userRepository.SaveChangesAsync(cancellationToken);

        return Ok(new AuthResponse
        {
            AccessToken = _jwtProvider.Generate(user),
            RefreshToken = newRefreshToken
        });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me([FromServices] ICurrentUser currentUser)
    {
        return Ok(new
        {
            currentUser.UserId,
            currentUser.Email,
            currentUser.Role
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin-only")]
    public IActionResult AdminOnly() => Ok("You are admin.");
}
