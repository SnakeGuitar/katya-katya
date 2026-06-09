using CommunityToolkit.Mvvm.ComponentModel;

namespace KatyaKatya.Services.Interfaces;

/// <summary>
/// Provides view-model-driven navigation within the single-window shell.
/// Maintains a history stack so any view can go back to where it came from.
/// </summary>
public interface INavigationService
{
    ObservableObject? CurrentViewModel { get; }
    bool IsAnimatedTransition { get; }
    bool CanGoBack { get; }

    void NavigateTo<TViewModel>() where TViewModel : ObservableObject;
    void NavigateTo<TViewModel>(Action<TViewModel> setup) where TViewModel : ObservableObject;
    void NavigateToRoot<TViewModel>() where TViewModel : ObservableObject;
    void NavigateToRoot<TViewModel>(Action<TViewModel> setup) where TViewModel : ObservableObject;
    void NavigateToRootWithFade<TViewModel>() where TViewModel : ObservableObject;
    void GoBack();
}
