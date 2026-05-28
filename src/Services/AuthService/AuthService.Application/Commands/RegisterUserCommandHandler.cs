using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.Exceptions;

namespace AuthService.Application.Commands;

public sealed class RegisterUserCommandHandler : IRegisterUserCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
    }

    public async Task<AuthResponse> HandleAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var exists = await _userRepository.ExistsByEmailAsync(command.Email, cancellationToken);
        if (exists)
            throw new UserAlreadyExistsException(command.Email);

        var passwordHash = _passwordHasher.Hash(command.Password);
        var user = User.Create(command.Email, passwordHash, command.Role);

        var refreshToken = _jwtProvider.GenerateRefreshToken();
        user.SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            AccessToken = _jwtProvider.Generate(user),
            RefreshToken = refreshToken,
            UserId = user.Id,
            Role = user.Role.ToString()
        };
    }
}
