using CommunityToolkit.Mvvm.ComponentModel;

namespace MemoryGame.Client.Services;

/// <summary>
/// Provides view-model-driven navigation within the single-window shell.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// The currently displayed view model, bound to the ContentControl in MainWindow.
    /// </summary>
    ObservableObject? CurrentViewModel { get; }

    /// <summary>
    /// Navigates to the view model of type <typeparamref name="TViewModel"/>.
    /// </summary>
    void NavigateTo<TViewModel>() where TViewModel : ObservableObject;

    /// <summary>
    /// Navigates to the view model of type <typeparamref name="TViewModel"/>,
    /// invoking the setup action before displaying it.
    /// </summary>
    void NavigateTo<TViewModel>(Action<TViewModel> setup) where TViewModel : ObservableObject;
}
