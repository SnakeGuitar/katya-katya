using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KatyaKatya.Services.Interfaces;
using KatyaKatya.Services.Network;
using KatyaKatya.Services.Core;
using KatyaKatya.Helpers;
using KatyaKatya.Localization;

namespace KatyaKatya.ViewModels.Session;

/// <summary>
/// Registration form view model.
/// </summary>
public partial class RegisterViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private readonly ApiClient _api;
    private readonly ClientSettings _settings;

    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _confirmPassword = string.Empty;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _isLoading;

    public string LogoPath => LogoResolver.Resolve(_settings.LanguageCode, _settings.ThemeName);

    public RegisterViewModel(INavigationService navigation, ApiClient api, ClientSettings settings)
    {
        _navigation = navigation;
        _api = api;
        _settings = settings;
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (Password != ConfirmPassword)
        {
            ErrorMessage = LocalizationManager.Instance["Session_Register_Error_PasswordMismatch"];
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        var result = await _api.PostAsync("api/auth/register", new
        {
            username = Username,
            email = Email,
            password = Password
        });

        IsLoading = false;

        if (result.IsSuccess)
        {
            _navigation.NavigateTo<VerifyEmailViewModel>(vm => vm.Email = Email);
        }
        else
        {
            ErrorMessage = ErrorResolver.Resolve(result.ErrorCode);
        }
    }

    [RelayCommand]
    private void GoBack() => _navigation.GoBack();
}
