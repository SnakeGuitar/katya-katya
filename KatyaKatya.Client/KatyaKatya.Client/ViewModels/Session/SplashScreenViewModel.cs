using CommunityToolkit.Mvvm.ComponentModel;
using KatyaKatya.Services.Interfaces;
using KatyaKatya.Services.Core;
using KatyaKatya.Helpers;

namespace KatyaKatya.ViewModels.Session;

public partial class SplashScreenViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private readonly ClientSettings _settings;

    public string LogoPath => LogoResolver.Resolve(_settings.LanguageCode, _settings.ThemeName);

    public SplashScreenViewModel(INavigationService navigation, ClientSettings settings)
    {
        _navigation = navigation;
        _settings = settings;
    }

    public void NavigateToTitleScreen()
    {
        // NavigateToRoot (no animation): ViewHost.Opacity stays 1 the entire time,
        // so the rounded-corner clip is never composited at partial opacity.
        _navigation.NavigateToRoot<TitleScreenViewModel>();
    }
}
