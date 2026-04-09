using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemoryGame.Client.Services.Core;
using MemoryGame.Client.Services.Interfaces;
using MemoryGame.Client.Services.Media;
using MemoryGame.Client.Services.Network;
using MemoryGame.Client.Services.UI;

namespace MemoryGame.Client.ViewModels.Session;

/// <summary>
/// Handles user registration via the REST API.
/// </summary>
public partial class RegisterViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private readonly ApiClient _api;

    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _confirmPassword = string.Empty;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private bool _isLoading;

    public RegisterViewModel(INavigationService navigation, ApiClient api)
    {
        _navigation = navigation;
        _api = api;
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        ErrorMessage = null;

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Passwords do not match.";
            return;
        }

        IsLoading = true;

        try
        {
            var result = await _api.PostAsync("api/auth/register", new { Username, Email, Password });

            if (!result.IsSuccess)
            {
                ErrorMessage = result.ErrorMessage ?? "Registration failed.";
                return;
            }

            _navigation.NavigateTo<VerifyEmailViewModel>(vm => vm.Email = Email);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void GoBack() => _navigation.GoBack();
}
