namespace AuthService.Application.DTOs;

public sealed class AuthResponse
{
    public string AccessToken { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
    public Guid UserId { get; set; }
    public string Role { get; set; } = default!;
}
