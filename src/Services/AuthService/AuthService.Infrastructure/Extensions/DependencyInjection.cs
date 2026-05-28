using AuthService.Application.Commands;
using AuthService.Application.Interfaces;
using AuthService.Application.Queries;
using AuthService.Infrastructure.Authentication;
using AuthService.Infrastructure.Persistence;
using AuthService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AuthDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtProvider, JwtProvider>();

        services.AddScoped<IRegisterUserCommandHandler, RegisterUserCommandHandler>();
        services.AddScoped<ILoginUserCommandHandler, LoginUserCommandHandler>();
        services.AddScoped<IRefreshTokenCommandHandler, RefreshTokenCommandHandler>();
        services.AddScoped<IGetCurrentUserQueryHandler, GetCurrentUserQueryHandler>();

        services.AddHttpContextAccessor();

        return services;
    }
}
