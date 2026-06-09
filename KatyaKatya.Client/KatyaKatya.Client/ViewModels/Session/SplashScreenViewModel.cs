using CommunityToolkit.Mvvm.ComponentModel;
using KatyaKatya.Services.Interfaces;
using KatyaKatya.Services.Core;
using KatyaKatya.Helpers;

namespace KatyaKatya.ViewModels.Session;

/// <summary>
/// Drives the splash screen timing.
/// </summary>
public partial class SplashScreenViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private readonly ClientSettings _settings;
    private const int HoldDelayMs = 2800;

    public event Action? FadeOutRequested;

    public string LogoPath => LogoResolver.Resolve(_settings.LanguageCode, _settings.ThemeName);

    public SplashScreenViewModel(INavigationService navigation, ClientSettings settings)
    {
        _navigation = navigation;
        _settings = settings;
    }

    public async Task StartAsync()
    {
        await Task.Delay(HoldDelayMs);
        FadeOutRequested?.Invoke();
    }

    public void NavigateToTitleScreen()
    {
        _navigation.NavigateToRootWithFade<TitleScreenViewModel>();
    }
}
