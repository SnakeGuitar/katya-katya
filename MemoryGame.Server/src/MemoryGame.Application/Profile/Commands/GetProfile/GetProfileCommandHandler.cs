using MediatR;
using MemoryGame.Application.Common.Interfaces;
using MemoryGame.Application.Profile.DTOs;
using MemoryGame.Domain.Common;
using MemoryGame.Domain.Users;

namespace MemoryGame.Application.Profile.Commands.GetProfile;

public class GetProfileCommandHandler : IRequestHandler<GetProfileCommand, ProfileResponse>
{
    private readonly IUserRepository _userRepository;

    public GetProfileCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ProfileResponse> Handle(GetProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId)
            ?? throw new DomainException("User not found.");

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
