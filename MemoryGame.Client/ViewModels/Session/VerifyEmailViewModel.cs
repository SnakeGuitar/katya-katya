using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemoryGame.Client.Services;

namespace MemoryGame.Client.ViewModels.Session;

/// <summary>
/// Handles email verification PIN entry after registration.
/// </summary>
public partial class VerifyEmailViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private readonly ApiClient _api;

    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _pin = string.Empty;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private string? _pinResentMessage;
    [ObservableProperty] private bool _isLoading;

    public VerifyEmailViewModel(INavigationService navigation, ApiClient api)
    {
        _navigation = navigation;
        _api = api;
    }

    [RelayCommand]
    private async Task VerifyAsync()
    {
        ErrorMessage = null;
        IsLoading = true;

        try
        {
            var result = await _api.PostAsync("api/auth/verify-email", new { Email, Pin });

            if (!result.IsSuccess)
            {
                ErrorMessage = result.ErrorMessage ?? "Verification failed.";
                return;
            }

            _navigation.NavigateTo<LoginViewModel>();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ResendPinAsync()
    {
        PinResentMessage = null;
        var result = await _api.PostAsync("api/auth/resend-verification", new { Email });
        if (result.IsSuccess)
            PinResentMessage = Localization.LocalizationManager.Instance["VerifyEmail_PinResentMessage"];
    }

    [RelayCommand]
    private void GoBack()
    {
        _navigation.NavigateTo<TitleScreenViewModel>();
    }
}
