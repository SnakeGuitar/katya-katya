using MediatR;

namespace MemoryGame.Application.Profile.Commands.ChangeUsername;

public record ChangeUsernameCommand(
    int UserId,
    string NewUsername
) : IRequest<Unit>;
