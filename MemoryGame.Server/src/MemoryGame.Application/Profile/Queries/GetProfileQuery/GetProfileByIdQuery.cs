using MediatR;
using MemoryGame.Application.Profile.DTOs;

namespace MemoryGame.Application.Profile.Queries.GetProfileQuery;

public record GetProfileByIdQuery(int UserId) : IRequest<ProfileResponse>;
