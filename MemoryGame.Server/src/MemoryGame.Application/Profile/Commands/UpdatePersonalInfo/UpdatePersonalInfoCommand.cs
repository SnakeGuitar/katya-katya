using MediatR;

namespace MemoryGame.Application.Profile.Commands.UpdatePersonalInfo;

public record UpdatePersonalInfoCommand(
    int UserId,
    string? Name,
    string? LastName
) : IRequest<Unit>;
