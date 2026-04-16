using MemoryGame.Client.Models;

namespace MemoryGame.Client.Services.Interfaces;

/// <summary>
/// Service for profile-related operations.
/// </summary>
public interface IProfileService
{
    Task<ApiResponse> UpdateAvatarAsync(byte[] avatarData);
    Task<ApiResponse> UpdatePersonalInfoAsync(string name, string lastName);
    Task<ApiResponse<SocialNetworkDto>> AddSocialNetworkAsync(string account);
    Task<ApiResponse> RemoveSocialNetworkAsync(int socialId);
    Task<ApiResponse> UpdateUsernameAsync(string newUsername);
    Task<ApiResponse> UpdatePasswordAsync(string currentPassword, string newPassword);
}
