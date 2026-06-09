using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KatyaKatya.Helpers;
using KatyaKatya.Localization;
using KatyaKatya.Models;
using KatyaKatya.Services.Interfaces;
using KatyaKatya.Services.Network;

namespace KatyaKatya.ViewModels.Social;

/// <summary>
/// Friends list + friend requests management.
/// </summary>
public partial class FriendsViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private readonly ISessionService _session;
    private readonly ApiClient _api;
    private readonly IDialogService _dialog;

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string? _errorMessage;

    public ObservableCollection<FriendDto> Friends { get; } = [];
    public ObservableCollection<FriendRequestDto> PendingRequests { get; } = [];

    [ObservableProperty] private string _searchUsername = string.Empty;
    [ObservableProperty] private bool _hasFriends;
    [ObservableProperty] private bool _hasRequests;

    public FriendsViewModel(
        INavigationService navigation,
        ISessionService session,
        ApiClient api,
        IDialogService dialog)
    {
        _navigation = navigation;
        _session = session;
        _api = api;
        _dialog = dialog;

        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var friendsTask = _api.GetAsync<FriendDto[]>("api/social/friends");
            var requestsTask = _api.GetAsync<FriendRequestDto[]>("api/social/friends/requests");

            await Task.WhenAll(friendsTask, requestsTask);

            Friends.Clear();
            var friendsResult = await friendsTask;
            if (friendsResult is { IsSuccess: true, Data: not null })
            {
                foreach (var f in friendsResult.Data)
                    Friends.Add(f);
            }
            HasFriends = Friends.Count > 0;

            PendingRequests.Clear();
            var requestsResult = await requestsTask;
            if (requestsResult is { IsSuccess: true, Data: not null })
            {
                foreach (var r in requestsResult.Data)
                    PendingRequests.Add(r);
            }
            HasRequests = PendingRequests.Count > 0;
        }
        catch (Exception)
        {
            ErrorMessage = LocalizationManager.Instance["Error_UNKNOWN"];
            _dialog.ShowMessage(ErrorMessage, LocalizationManager.Instance["Global_Title_Error"], DialogButton.OK, DialogIcon.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ── Send friend request ─────────────────────────────────

    [RelayCommand]
    private async Task SendRequestAsync()
    {
        var username = SearchUsername.Trim();
        if (string.IsNullOrEmpty(username)) return;

        if (username == _session.Current?.Username)
        {
            _dialog.ShowMessage(LocalizationManager.Instance["Error_SOCIAL_CANNOT_ADD_SELF"],
                LocalizationManager.Instance["Global_Title_Error"], DialogButton.OK, DialogIcon.Error);
            return;
        }

        IsLoading = true;
        try
        {
            var result = await _api.PostAsync("api/social/friends/request", new { ReceiverUsername = username });

            if (result.IsSuccess)
            {
                _dialog.ShowMessage(LocalizationManager.Instance.Format("Friends_Message_RequestSent", username),
                    LocalizationManager.Instance["Global_Title_Success"], DialogButton.OK, DialogIcon.Information);
                SearchUsername = string.Empty;
            }
            else
            {
                var errMsg = ErrorResolver.Resolve(result.ErrorCode);
                _dialog.ShowMessage(errMsg, LocalizationManager.Instance["Global_Title_Error"], DialogButton.OK, DialogIcon.Error);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Friends] Send request failed: {ex.Message}");
            _dialog.ShowMessage(LocalizationManager.Instance["Error_UNKNOWN"],
                LocalizationManager.Instance["Global_Title_Error"], DialogButton.OK, DialogIcon.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ── Accept / Reject requests ────────────────────────────

    [RelayCommand]
    private async Task AcceptRequestAsync(int requestId)
    {
        IsLoading = true;
        try
        {
            var result = await _api.PostAsync("api/social/friends/request/answer", new { RequestId = requestId, Accept = true });

            if (result.IsSuccess)
            {
                _dialog.ShowMessage(LocalizationManager.Instance["Global_Title_Success"],
                    LocalizationManager.Instance["Global_Title_Success"], DialogButton.OK, DialogIcon.Information);
                await LoadDataAsync();
            }
            else
            {
                var errMsg = ErrorResolver.Resolve(result.ErrorCode);
                _dialog.ShowMessage(errMsg, LocalizationManager.Instance["Global_Title_Error"], DialogButton.OK, DialogIcon.Error);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Friends] Accept request failed: {ex.Message}");
            _dialog.ShowMessage(LocalizationManager.Instance["Error_UNKNOWN"],
                LocalizationManager.Instance["Global_Title_Error"], DialogButton.OK, DialogIcon.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RejectRequestAsync(int requestId)
    {
        IsLoading = true;
        try
        {
            var result = await _api.PostAsync("api/social/friends/request/answer", new { RequestId = requestId, Accept = false });

            if (result.IsSuccess)
            {
                _dialog.ShowMessage(LocalizationManager.Instance["Global_Title_Information"],
                    LocalizationManager.Instance["Global_Title_Information"], DialogButton.OK, DialogIcon.Information);
                await LoadDataAsync();
            }
            else
            {
                var errMsg = ErrorResolver.Resolve(result.ErrorCode);
                _dialog.ShowMessage(errMsg, LocalizationManager.Instance["Global_Title_Error"], DialogButton.OK, DialogIcon.Error);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Friends] Reject request failed: {ex.Message}");
            _dialog.ShowMessage(LocalizationManager.Instance["Error_UNKNOWN"],
                LocalizationManager.Instance["Global_Title_Error"], DialogButton.OK, DialogIcon.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ── Remove friend ───────────────────────────────────────

    [RelayCommand]
    private async Task RemoveFriendAsync(FriendDto friend)
    {
        var confirm = _dialog.ShowMessage(
            LocalizationManager.Instance.Format("Friends_Message_RemoveFriend", friend.Username),
            LocalizationManager.Instance["Global_Title_Confirm"],
            DialogButton.YesNo, DialogIcon.Question);

        if (confirm != DialogResult.Yes) return;

        IsLoading = true;
        try
        {
            var result = await _api.DeleteAsync($"api/social/friends/{friend.UserId}");
            if (result.IsSuccess)
            {
                _dialog.ShowMessage(LocalizationManager.Instance["Global_Title_Success"],
                    LocalizationManager.Instance["Global_Title_Success"], DialogButton.OK, DialogIcon.Information);
                await LoadDataAsync();
            }
            else
            {
                var errMsg = ErrorResolver.Resolve(result.ErrorCode);
                _dialog.ShowMessage(errMsg, LocalizationManager.Instance["Global_Title_Error"], DialogButton.OK, DialogIcon.Error);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Friends] Remove friend failed: {ex.Message}");
            _dialog.ShowMessage(LocalizationManager.Instance["Error_UNKNOWN"],
                LocalizationManager.Instance["Global_Title_Error"], DialogButton.OK, DialogIcon.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void GoBack() => _navigation.GoBack();
}
