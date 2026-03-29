using MediatR;

namespace MemoryGame.Application.Profile.Queries.GetUserAvatarQuery;

    public record GetUserAvatarQuery(
    int UserId
) : IRequest<byte[]?>;