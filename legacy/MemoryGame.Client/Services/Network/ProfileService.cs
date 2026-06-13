using MemoryGame.Client.Models;
using MemoryGame.Client.Services.Interfaces;

namespace MemoryGame.Client.Services.Network;

public class ProfileService : IProfileService
{
    private readonly ApiClient _api;

    public ProfileService(ApiClient api)
    {
        _api = api;
    }

    public Task<ApiResponse> UpdateAvatarAsync(byte[] avatarData) =>
        _api.PutAsync("api/profile/avatar", new { AvatarData = avatarData });

    public Task<ApiResponse> UpdatePersonalInfoAsync(string name, string lastName) =>
        _api.PutAsync("api/profile/personal-info", new { Name = name, LastName = lastName });

    public Task<ApiResponse<SocialNetworkDto>> AddSocialNetworkAsync(string account) =>
        _api.PostAsync<SocialNetworkDto>("api/social/networks", new { Account = account });

    public Task<ApiResponse> RemoveSocialNetworkAsync(int socialId) =>
        _api.DeleteAsync($"api/social/networks/{socialId}");

    public Task<ApiResponse> UpdateUsernameAsync(string newUsername) =>
        _api.PutAsync("api/profile/username", new { NewUsername = newUsername });

    public Task<ApiResponse> UpdatePasswordAsync(string currentPassword, string newPassword) =>
        _api.PutAsync("api/profile/password", new { CurrentPassword = currentPassword, NewPassword = newPassword });
}
