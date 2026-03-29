using MediatR;
using MemoryGame.Application.Profile.DTOs;

namespace MemoryGame.Application.Profile.Commands.GetProfile;

public record GetProfileCommand(
    int UserId
) : IRequest<ProfileResponse>;
