using MediatR;
using KatyaKatya.Application.Profile.DTOs;

namespace KatyaKatya.Application.Profile.Queries.GetProfileQuery;

public record GetProfileByIdQuery(int UserId) : IRequest<ProfileResponse>;
