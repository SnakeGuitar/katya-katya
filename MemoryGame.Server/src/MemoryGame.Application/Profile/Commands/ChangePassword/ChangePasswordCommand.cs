using MediatR;

namespace MemoryGame.Application.Profile.Commands.ChangePassword;

public record ChangePasswordCommand(
    int UserId,
    string CurrentPassword,
    string NewPassword
) : IRequest<Unit>;
