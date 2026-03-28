using MediatR;

namespace MemoryGame.Application.Auth.Commands.Logout;

public record LogoutCommand(
    int UserId
) : IRequest<Unit>;
