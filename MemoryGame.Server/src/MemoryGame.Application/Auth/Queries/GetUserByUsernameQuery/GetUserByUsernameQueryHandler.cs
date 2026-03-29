using MediatR;
using MemoryGame.Application.Auth.DTOs;
using MemoryGame.Domain.Common;
using MemoryGame.Domain.Users;

namespace MemoryGame.Application.Auth.Queries.GetUserByUsernameQuery;

/// <summary>
/// Handles <see cref="GetUserByUsernameQuery"/>: retrieves a user by their username.
/// </summary>
public class GetUserByUsernameQueryHandler : IRequestHandler<GetUserByUsernameQuery, UserDto>
{
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Initializes the handler with its dependencies.
    /// </summary>
    public GetUserByUsernameQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <inheritdoc/>
    public async Task<UserDto> Handle(GetUserByUsernameQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username)
            ?? throw new DomainException(DomainErrors.User.NotFound);

        return new UserDto(user.Id, user.Username, user.Email.Value, user.IsGuest, user.VerifiedEmail);
    }
}
