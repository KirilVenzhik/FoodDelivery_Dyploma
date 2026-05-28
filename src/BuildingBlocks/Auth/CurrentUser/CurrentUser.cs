using System.Security.Claims;
using Auth.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Auth.CurrentUser;

public sealed class CurrentUser
    : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var value = _httpContextAccessor
                .HttpContext?
                .User
                .FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.Parse(value!);
        }
    }

    public string Email =>
        _httpContextAccessor
            .HttpContext?
            .User
            .FindFirstValue(ClaimTypes.Email)!;

    public string Role =>
        _httpContextAccessor
            .HttpContext?
            .User
            .FindFirstValue(ClaimTypes.Role)!;
}