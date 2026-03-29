using MediatR;
using MemoryGame.Application.Profile.DTOs;
using MemoryGame.Domain.Common;
using MemoryGame.Domain.Users;

namespace MemoryGame.Application.Profile.Queries.GetProfileQuery;

/// <summary>
/// Handles <see cref="GetProfileByIdQuery"/>: retrieves the full profile of the user.
/// </summary>
public class GetProfileByIdQueryHandler : IRequestHandler<GetProfileByIdQuery, ProfileResponse>
{
    private readonly IUserRepository _userRepository;

    public GetProfileByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <inheritdoc/>
    public async Task<ProfileResponse> Handle(GetProfileByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId)
            ?? throw new DomainException(DomainErrors.User.NotFound);

        return new ProfileResponse(
            Id: user.Id,
            Username: user.Username,
            Name: user.Name,
            LastName: user.LastName,
            Email: user.Email.Value,
            IsGuest: user.IsGuest,
            VerifiedEmail: user.VerifiedEmail,
            RegistrationDate: user.RegistrationDate,
            Avatar: user.Avatar);
    }
}
