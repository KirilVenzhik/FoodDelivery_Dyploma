using Auth.Abstractions;
using Auth.CurrentUser;
using Microsoft.Extensions.DependencyInjection;

namespace Auth;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthServices(
        this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.AddScoped<
            ICurrentUser,
            CurrentUser.CurrentUser>();

        return services;
    }
}