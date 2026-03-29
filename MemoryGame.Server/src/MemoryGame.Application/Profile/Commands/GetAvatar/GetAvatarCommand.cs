using MediatR;

namespace MemoryGame.Application.Profile.Commands.GetAvatar;

public record GetAvatarCommand(
    int UserId
) : IRequest<byte[]?>;
