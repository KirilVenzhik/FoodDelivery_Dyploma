namespace AuthService.Application.DTOs;

public sealed class AuthResponse
{
    public string AccessToken { get; set; } = default!;
}