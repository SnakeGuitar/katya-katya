using CommunityToolkit.Mvvm.ComponentModel;

namespace MemoryGame.Client.Services.Interfaces;

/// <summary>
/// Provides view-model-driven navigation within the single-window shell.
/// Maintains a history stack so any view can go back to where it came from.
/// </summary>
public interface INavigationService
{
    /// <summary>The currently displayed view model.</summary>
    ObservableObject? CurrentViewModel { get; }

    /// <summary>True when the pending navigation was requested with a fade transition.</summary>
    bool IsAnimatedTransition { get; }

    /// <summary>True when there is a previous entry to return to.</summary>
    bool CanGoBack { get; }

    /// <summary>
    /// Forward navigation — pushes the current view onto the history stack,
    /// then displays the requested view model.
    /// </summary>
    void NavigateTo<TViewModel>() where TViewModel : ObservableObject;

    /// <inheritdoc cref="NavigateTo{TViewModel}()"/>
    void NavigateTo<TViewModel>(Action<TViewModel> setup) where TViewModel : ObservableObject;

    /// <summary>
    /// Root navigation — clears the history stack and sets the given view as the new root.
    /// Use for checkpoints where going "back" should not be possible (e.g. post-login, logout).
    /// </summary>
    void NavigateToRoot<TViewModel>() where TViewModel : ObservableObject;

    /// <inheritdoc cref="NavigateToRoot{TViewModel}()"/>
    void NavigateToRoot<TViewModel>(Action<TViewModel> setup) where TViewModel : ObservableObject;

    /// <summary>
    /// Same as <see cref="NavigateToRoot{TViewModel}()"/> but signals the shell to play a fade transition.
    /// </summary>
    void NavigateToRootWithFade<TViewModel>() where TViewModel : ObservableObject;

    /// <summary>
    /// Goes back to the previous view model, if one exists.
    /// Falls back to the title screen if the stack is empty.
    /// </summary>
    void GoBack();
}
