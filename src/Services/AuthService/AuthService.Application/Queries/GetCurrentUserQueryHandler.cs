using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Exceptions;

namespace AuthService.Application.Queries;

public sealed class GetCurrentUserQueryHandler : IGetCurrentUserQueryHandler
{
    private readonly IUserRepository _userRepository;

    public GetCurrentUserQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto> HandleAsync(
        GetCurrentUserQuery query,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(query.UserId, cancellationToken);
        if (user is null)
            throw new UserNotFoundException(query.UserId);

        return new UserDto
        {
            UserId = user.Id,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }
}
