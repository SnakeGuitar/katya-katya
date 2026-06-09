using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KatyaKatya.Services.Interfaces;
using KatyaKatya.Services.Core;
using KatyaKatya.Helpers;

namespace KatyaKatya.ViewModels.Session;

/// <summary>
/// Title screen with login, register, or guest options.
/// </summary>
public partial class TitleScreenViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private readonly ClientSettings _settings;

    public string LogoPath => LogoResolver.Resolve(_settings.LanguageCode, _settings.ThemeName);

    public TitleScreenViewModel(INavigationService navigation, ClientSettings settings)
    {
        _navigation = navigation;
        _settings = settings;
    }

    [RelayCommand]
    private void GoToLogin() => _navigation.NavigateTo<LoginViewModel>();

    [RelayCommand]
    private void GoToRegister() => _navigation.NavigateTo<RegisterViewModel>();

    [RelayCommand]
    private void GoToGuestLogin() => _navigation.NavigateTo<GuestLoginViewModel>();
}
