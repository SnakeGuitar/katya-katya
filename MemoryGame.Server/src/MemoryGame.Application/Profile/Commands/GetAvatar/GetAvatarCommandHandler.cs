using MediatR;
using MemoryGame.Application.Common.Interfaces;
using MemoryGame.Domain.Common;
using MemoryGame.Domain.Users;

namespace MemoryGame.Application.Profile.Commands.GetAvatar;

public class GetAvatarCommandHandler : IRequestHandler<GetAvatarCommand, byte[]?>
{
    private readonly IUserRepository _userRepository;

    public GetAvatarCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<byte[]?> Handle(GetAvatarCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId)
            ?? throw new DomainException("User not found.");

        return user.Avatar;
    }
}
