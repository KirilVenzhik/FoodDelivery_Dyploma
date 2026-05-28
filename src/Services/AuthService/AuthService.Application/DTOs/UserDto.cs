namespace AuthService.Application.DTOs;

public sealed class UserDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = default!;
    public string Role { get; set; } = default!;
}
