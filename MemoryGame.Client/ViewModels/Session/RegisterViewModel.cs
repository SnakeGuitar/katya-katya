using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemoryGame.Client.Services.Core;
using MemoryGame.Client.Services.Interfaces;
using MemoryGame.Client.Services.Media;
using MemoryGame.Client.Services.Network;
using MemoryGame.Client.Services.UI;

using MemoryGame.Client.ViewModels.Common;

namespace MemoryGame.Client.ViewModels.Session;



/// <summary>
/// Handles user registration via the REST API.
/// </summary>
public partial class RegisterViewModel : BaseViewModel
{
    private readonly ApiClient _api;

    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _confirmPassword = string.Empty;


    public RegisterViewModel(INavigationService navigation, IDialogService dialog, ApiClient api) : base(navigation, dialog)
    {
        _api = api;
    }


    [RelayCommand]
    private Task RegisterAsync() => RunAsync(async () =>
    {
        ErrorMessage = null;

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Passwords do not match.";
            return;
        }

        var result = await _api.PostAsync("api/auth/register", new { Username, Email, Password });

        if (!result.IsSuccess)
        {
            ErrorMessage = result.ErrorMessage ?? "Registration failed.";
            return;
        }

        Navigation.NavigateTo<VerifyEmailViewModel>(vm => vm.Email = Email);
    });
}

