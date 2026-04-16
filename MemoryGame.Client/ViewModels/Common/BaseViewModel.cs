using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemoryGame.Client.Localization;
using MemoryGame.Client.Models;
using MemoryGame.Client.Services.Interfaces;

namespace MemoryGame.Client.ViewModels.Common;

/// <summary>
/// Base view model that centralizes loading states, error handling, 
/// dialog popups, and navigation to avoid boilerplate in every screen.
/// </summary>
public abstract partial class BaseViewModel : ObservableObject
{
    protected readonly INavigationService Navigation;
    protected readonly IDialogService Dialog;

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string? _errorMessage;

    protected BaseViewModel(INavigationService navigation, IDialogService dialog)
    {
        Navigation = navigation;
        Dialog = dialog;
    }

    [RelayCommand]
    protected virtual void GoBack() => Navigation.GoBack();

    /// <summary>
    /// Safely executes an async task, automatically handling the IsLoading state
    /// and showing default error dialogs if an unhandled exception occurs.
    /// </summary>
    protected async Task RunAsync(Func<Task> action, bool showDefaultErrorIndicator = true)
    {
        if (IsLoading) return; // Prevent double-clicking
        
        IsLoading = true;
        ErrorMessage = null;
        
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            if (showDefaultErrorIndicator)
            {
                Dialog.ShowMessage(
                    LocalizationManager.Instance["Error_UNKNOWN"],
                    LocalizationManager.Instance["Global_Title_Error"],
                    DialogButton.OK, DialogIcon.Error);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Unified API response handler to show success/error popups gracefully.
    /// </summary>
    protected async Task<bool> HandleResponseAsync(Task<ApiResponse> task, string? successKey = null)
        => await HandleResponseAsync(await task, successKey);

    protected Task<bool> HandleResponseAsync(ApiResponse result, string? successKey = null)
    {
        if (result.IsSuccess)
        {
            if (successKey != null)
            {
                Dialog.ShowMessage(LocalizationManager.Instance[successKey],
                    LocalizationManager.Instance["Global_Title_Success"],
                    DialogButton.OK, DialogIcon.Information);
            }
            return Task.FromResult(true);
        }

        Dialog.ShowMessage(ErrorResolver.Resolve(result.ErrorCode),
            LocalizationManager.Instance["Global_Title_Error"],
            DialogButton.OK, DialogIcon.Error);
        return Task.FromResult(false);
    }

    protected async Task<bool> HandleResponseAsync<T>(Task<ApiResponse<T>> task, string? successKey = null)
        => await HandleResponseAsync(await task, successKey);

    protected Task<bool> HandleResponseAsync<T>(ApiResponse<T> result, string? successKey = null)
    {
        if (result.IsSuccess)
        {
            if (successKey != null)
            {
                Dialog.ShowMessage(LocalizationManager.Instance[successKey],
                    LocalizationManager.Instance["Global_Title_Success"],
                    DialogButton.OK, DialogIcon.Information);
            }
            return Task.FromResult(true);
        }

        Dialog.ShowMessage(ErrorResolver.Resolve(result.ErrorCode),
            LocalizationManager.Instance["Global_Title_Error"],
            DialogButton.OK, DialogIcon.Error);
        return Task.FromResult(false);
    }


    protected void ShowWarning(string localizationKey)
    {
        Dialog.ShowMessage(LocalizationManager.Instance[localizationKey],
            LocalizationManager.Instance["Global_Title_Warning"],
            DialogButton.OK, DialogIcon.Warning);
    }
}
