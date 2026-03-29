using MediatR;

namespace MemoryGame.Application.Profile.Commands.UpdateAvatar;

public record UpdateAvatarCommand(
    int UserId,
    byte[] AvatarData
) : IRequest<Unit>;
