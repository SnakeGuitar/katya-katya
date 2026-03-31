using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemoryGame.Client.Services;

namespace MemoryGame.Client.ViewModels.Session;

/// <summary>
/// Title screen with options to login, register, or play as guest.
/// </summary>
public partial class TitleScreenViewModel : ObservableObject
{
    private readonly INavigationService _navigation;

    public TitleScreenViewModel(INavigationService navigation)
    {
        _navigation = navigation;
    }

    [RelayCommand]
    private void GoToLogin()
    {
        _navigation.NavigateTo<LoginViewModel>();
    }

    [RelayCommand]
    private void GoToRegister()
    {
        _navigation.NavigateTo<RegisterViewModel>();
    }
}
