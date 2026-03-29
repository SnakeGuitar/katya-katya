using MediatR;
using MemoryGame.Domain.Common;
using MemoryGame.Domain.Users;
namespace MemoryGame.Application.Profile.Queries.GetUserAvatarQuery;

/// <summary>
/// Handles <see cref="GetUserAvatarQuery"/>: retrieves the avatar bytes of the user.
/// </summary>
public class GetUserAvatarQueryHandler : IRequestHandler<GetUserAvatarQuery, byte[]?>
{
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Initializes the handler with its dependencies.
    /// </summary>
    public GetUserAvatarQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <inheritdoc/>
    public async Task<byte[]?> Handle(GetUserAvatarQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId)
            ?? throw new DomainException(DomainErrors.User.NotFound);

        return user.Avatar;
    }
}
