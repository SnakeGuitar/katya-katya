using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemoryGame.Client.Services.Core;
using MemoryGame.Client.Services.Interfaces;
using MemoryGame.Client.Services.Media;
using MemoryGame.Client.Services.Network;
using MemoryGame.Client.Services.UI;

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

    [RelayCommand]
    private void GoToGuestLogin()
    {
        _navigation.NavigateTo<GuestLoginViewModel>();
    }
}
