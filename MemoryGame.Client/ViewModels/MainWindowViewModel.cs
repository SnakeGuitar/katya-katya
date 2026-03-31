using CommunityToolkit.Mvvm.ComponentModel;
using MemoryGame.Client.Services;

namespace MemoryGame.Client.ViewModels;

/// <summary>
/// Shell view model. Holds the current view via the navigation service.
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    private readonly INavigationService _navigation;

    public MainWindowViewModel(INavigationService navigation)
    {
        _navigation = navigation;
    }

    /// <summary>
    /// The navigation service that exposes the current view model to the shell.
    /// </summary>
    public INavigationService Navigation => _navigation;
}
