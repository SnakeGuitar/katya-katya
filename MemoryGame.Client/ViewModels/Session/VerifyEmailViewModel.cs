using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemoryGame.Client.Services.Core;
using MemoryGame.Client.Services.Interfaces;
using MemoryGame.Client.Services.Media;
using MemoryGame.Client.Services.Network;
using MemoryGame.Client.Services.UI;

namespace MemoryGame.Client.ViewModels.Session;

/// <summary>
/// Handles email verification PIN entry after registration.
/// After a successful PIN, the user is automatically logged in.
/// </summary>
public partial class VerifyEmailViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private readonly ApiClient          _api;

    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _pin = string.Empty;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private string? _pinResentMessage;
    [ObservableProperty] private bool _isLoading;

    public VerifyEmailViewModel(
        INavigationService navigation,
        ApiClient          api)
    {
        _navigation = navigation;
        _api        = api;
    }

    [RelayCommand]
    private async Task VerifyAsync()
    {
        ErrorMessage = null;
        IsLoading = true;

        try
        {
            var result = await _api.PostAsync<VerifyRegistrationResponse>(
                "api/auth/verify-registration", new { Email, Pin });

            if (!result.IsSuccess)
            {
                ErrorMessage = result.ErrorMessage ?? "Verification failed.";
                return;
            }

            if (!result.Data!.Valid)
            {
                ErrorMessage = Localization.LocalizationManager.Instance["VerifyEmail_InvalidPin"];
                return;
            }

            _navigation.NavigateTo<SetupProfileViewModel>(vm =>
            {
                vm.Email = Email;
                vm.Pin = Pin;
            });
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
    private void GoBack() => _navigation.GoBack();
}

/// <summary>
/// Response from the verify-registration endpoint.
/// </summary>
public record VerifyRegistrationResponse(bool Valid);

/// <summary>
/// Response from the finalize-registration endpoint.
/// </summary>
public record FinalizeRegistrationResponse(
    string          AccessToken,
    string          RefreshToken,
    FinalizeUserDto User);

/// <summary>
/// User information returned when finalizing registration.
/// </summary>
public record FinalizeUserDto(
    int    Id,
    string Username,
    string Email,
    bool   IsGuest,
    bool   VerifiedEmail);
