using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemoryGame.Client.Services;
using MemoryGame.Client.ViewModels.Settings;

namespace MemoryGame.Client.ViewModels;

/// <summary>
/// Shell view model. Holds the current view via the navigation service
/// and exposes window-level commands (e.g. F11 fullscreen toggle).
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private readonly IWindowService     _window;

    public MainWindowViewModel(INavigationService navigation, IWindowService window)
    {
        _navigation = navigation;
        _window     = window;
    }

    /// <summary>The navigation service that exposes the current view model to the shell.</summary>
    public INavigationService Navigation => _navigation;

    /// <summary>Bound to the global settings button in MainWindow.xaml.</summary>
    [RelayCommand]
    private void GoToSettings() => _navigation.NavigateTo<SettingsViewModel>();

    /// <summary>Bound to the F11 KeyBinding in MainWindow.xaml.</summary>
    [RelayCommand]
    private void ToggleFullscreen() => _window.ToggleFullscreen();
}
