using CommunityToolkit.Mvvm.ComponentModel;
using MemoryGame.Client.Models;
using MemoryGame.Client.Services.Network;

namespace MemoryGame.Client.Helpers;

public partial class ProfileLoader : ObservableObject
{
    private readonly ApiClient _api;

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private byte[]? _avatar;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _lastName = string.Empty;
    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private DateTime _registrationDate;

    public ProfileResponse? Profile { get; private set; }

    public SocialNetworkDto[]? SocialNetworks { get; private set; }

    public ProfileLoader(ApiClient api)
    {
        _api = api;
    }

    public async Task LoadAllAsync()
    {
        IsLoading = true;
        try
        {
            var profileTask = _api.GetAsync<ProfileResponse>("api/profile");
            var socialsTask = _api.GetAsync<SocialNetworkDto[]>("api/social/networks");

            await Task.WhenAll(profileTask, socialsTask);

            var pRes = await profileTask;
            var sRes = await socialsTask;

            if (pRes is { IsSuccess: true, Data: not null })
            {
                Profile = pRes.Data;
                Avatar = Profile.Avatar;
                Name = Profile.Name ?? string.Empty;
                LastName = Profile.LastName ?? string.Empty;
                Username = Profile.Username;
                Email = Profile.Email;
                RegistrationDate = Profile.RegistrationDate;
            }

            if (sRes is { IsSuccess: true, Data: not null })
                SocialNetworks = sRes.Data;
        }
        finally
        {
            IsLoading = false;
        }
    }
}


