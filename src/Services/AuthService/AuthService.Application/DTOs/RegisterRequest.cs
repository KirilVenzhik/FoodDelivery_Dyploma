using AuthService.Domain.Enums;

namespace AuthService.Application.DTOs;

public sealed class RegisterRequest
{
    public string Email { get; set; } = default!;

    public string Password { get; set; } = default!;

    public UserRole Role { get; set; }
}