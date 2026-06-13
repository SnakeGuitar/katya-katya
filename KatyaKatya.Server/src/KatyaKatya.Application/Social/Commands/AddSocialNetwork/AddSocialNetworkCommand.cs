using MediatR;
using KatyaKatya.Application.Social.DTOs;

namespace KatyaKatya.Application.Social.Commands.AddSocialNetwork;

/// <summary>
/// Links a new social network account to a user's profile.
/// </summary>
/// <param name="UserId">The user identifier.</param>
/// <param name="Account">The social network account name or handle.</param>
public record AddSocialNetworkCommand(int UserId, string Account) : IRequest<SocialNetworkDto>;
