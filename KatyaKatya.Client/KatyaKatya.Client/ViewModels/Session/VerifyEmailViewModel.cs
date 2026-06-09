using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KatyaKatya.Localization;
using KatyaKatya.Services.Interfaces;
using KatyaKatya.Services.Network;

namespace KatyaKatya.ViewModels.Session;

/// <summary>
/// Handles email-verification PIN entry after registration.
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
        PinResentMessage = null;
        IsLoading = true;

        var result = await _api.PostAsync<VerifyRegistrationResponse>(
            "api/auth/verify-registration", new { Email, Pin });

        IsLoading = false;

        if (!result.IsSuccess)
        {
            ErrorMessage = result.ErrorMessage ?? LocalizationManager.Instance["Error_UNKNOWN"];
            return;
        }

        if (!result.Data!.Valid)
        {
            ErrorMessage = LocalizationManager.Instance["Error_AUTH_PIN_INVALID"];
            return;
        }

        _navigation.NavigateTo<SetupProfileViewModel>(vm =>
        {
            vm.Email = Email;
            vm.Pin = Pin;
        });
    }

    [RelayCommand]
    private async Task ResendPinAsync()
    {
        PinResentMessage = null;
        var result = await _api.PostAsync("api/auth/resend-verification", new { Email });
        if (result.IsSuccess)
            PinResentMessage = LocalizationManager.Instance["VerifyEmail_PinResentMessage"];
    }

    [RelayCommand]
    private void GoBack() => _navigation.GoBack();
}

public record VerifyRegistrationResponse(bool Valid);
