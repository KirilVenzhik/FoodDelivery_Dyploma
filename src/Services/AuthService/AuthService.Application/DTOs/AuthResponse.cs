namespace AuthService.Application.DTOs;

public sealed class AuthResponse
{
    public string AccessToken { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
}