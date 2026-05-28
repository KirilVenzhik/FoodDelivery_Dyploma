using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Auth.Abstractions;

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
            .ExistsByEmailAsync(
                request.Email,
                cancellationToken);

        if (exists)
        {
            return Conflict("Email already exists.");
        }

        var passwordHash = _passwordHasher
            .Hash(request.Password);

        var user = Domain.Entities.User.Create(
            request.Email,
            passwordHash,
            request.Role);

        await _userRepository.AddAsync(
            user,
            cancellationToken);

        await _userRepository.SaveChangesAsync(
            cancellationToken);

        var token = _jwtProvider.Generate(user);

        return Ok(new AuthResponse
        {
            AccessToken = token
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository
            .GetByEmailAsync(
                request.Email,
                cancellationToken);

        if (user is null)
        {
            return Unauthorized();
        }

        var validPassword = _passwordHasher
            .Verify(
                request.Password,
                user.PasswordHash);

        if (!validPassword)
        {
            return Unauthorized();
        }

        var token = _jwtProvider.Generate(user);

        return Ok(new AuthResponse
        {
            AccessToken = token
        });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me(
    [FromServices] ICurrentUser currentUser)
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
    public IActionResult AdminOnly()
    {
        return Ok("You are admin.");
    }
}