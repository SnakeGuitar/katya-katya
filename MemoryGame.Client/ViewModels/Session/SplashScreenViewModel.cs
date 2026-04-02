using CommunityToolkit.Mvvm.ComponentModel;
using MemoryGame.Client.Services;

namespace MemoryGame.Client.ViewModels.Session;

/// <summary>
/// Drives the splash screen timing.
/// Fires <see cref="FadeOutRequested"/> when the hold period is over;
/// the View starts the fade-out animation and calls <see cref="NavigateToTitleScreen"/>
/// once that animation completes.
/// Fade-in: 1.2 s  |  Hold: 1.6 s  →  trigger fade-out after 2.8 s total
/// </summary>
public partial class SplashScreenViewModel : ObservableObject
{
    private readonly INavigationService _navigation;

    private const int HoldDelayMs = 2800;

    /// <summary>Raised on the UI thread when the View should start its fade-out.</summary>
    public event Action? FadeOutRequested;

    public SplashScreenViewModel(INavigationService navigation)
    {
        _navigation = navigation;
    }

    /// <summary>Called by the View after it is loaded to start the clock.</summary>
    public async Task StartAsync()
    {
        await Task.Delay(HoldDelayMs);
        FadeOutRequested?.Invoke();
    }

    /// <summary>Called by the View once the fade-out animation completes.</summary>
    public void NavigateToTitleScreen()
    {
        _navigation.NavigateToRootWithFade<TitleScreenViewModel>();
    }
}
