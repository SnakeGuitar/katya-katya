using MediatR;

namespace KatyaKatya.Application.Profile.Queries.GetUserAvatarQuery;

    public record GetUserAvatarQuery(
    int UserId
) : IRequest<byte[]?>;