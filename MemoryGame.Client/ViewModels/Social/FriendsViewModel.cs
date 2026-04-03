using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemoryGame.Client.Localization;
using MemoryGame.Client.Models;
using MemoryGame.Client.Services;

namespace MemoryGame.Client.ViewModels.Social;

/// <summary>
/// Friends list + friend requests management.
/// Mirrors the legacy FriendsMenu with two tabs: My Friends / Requests.
/// </summary>
public partial class FriendsViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private readonly ISessionService _session;
    private readonly ApiClient _api;
    private readonly IDialogService _dialog;

    public ObservableCollection<FriendDto> Friends { get; } = [];
    public ObservableCollection<FriendRequestDto> PendingRequests { get; } = [];

    [ObservableProperty] private string _searchUsername = string.Empty;
    [ObservableProperty] private bool _isLoading;
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
        try
        {
            var friendsTask = _api.GetAsync<FriendDto[]>("api/social/friends");
            var requestsTask = _api.GetAsync<FriendRequestDto[]>("api/social/friends/requests");

            await Task.WhenAll(friendsTask, requestsTask);

            Friends.Clear();
            var friendsResult = friendsTask.Result;
            if (friendsResult is { IsSuccess: true, Data: not null })
            {
                foreach (var f in friendsResult.Data)
                    Friends.Add(f);
            }
            HasFriends = Friends.Count > 0;

            PendingRequests.Clear();
            var requestsResult = requestsTask.Result;
            if (requestsResult is { IsSuccess: true, Data: not null })
            {
                foreach (var r in requestsResult.Data)
                    PendingRequests.Add(r);
            }
            HasRequests = PendingRequests.Count > 0;
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
            _dialog.ShowMessage(
                LocalizationManager.Instance["Error_SOCIAL_CANNOT_ADD_SELF"],
                LocalizationManager.Instance["Global_Title_Error"],
                DialogButton.OK, DialogIcon.Error);
            return;
        }

        var result = await _api.PostAsync("api/social/friends/request",
            new { ReceiverUsername = username });

        if (result.IsSuccess)
        {
            _dialog.ShowMessage(
                LocalizationManager.Instance.Format("Friends_Message_RequestSent", username),
                LocalizationManager.Instance["Global_Title_Success"],
                DialogButton.OK, DialogIcon.Information);
            SearchUsername = string.Empty;
        }
        else
        {
            _dialog.ShowMessage(
                ErrorResolver.Resolve(result.ErrorCode),
                LocalizationManager.Instance["Global_Title_Error"],
                DialogButton.OK, DialogIcon.Error);
        }
    }

    // ── Accept / Reject requests ────────────────────────────

    [RelayCommand]
    private async Task AcceptRequestAsync(int requestId)
    {
        var result = await _api.PostAsync("api/social/friends/request/answer",
            new { RequestId = requestId, Accept = true });

        if (result.IsSuccess)
            await LoadDataAsync();
        else
            ShowApiError(result.ErrorCode);
    }

    [RelayCommand]
    private async Task RejectRequestAsync(int requestId)
    {
        var result = await _api.PostAsync("api/social/friends/request/answer",
            new { RequestId = requestId, Accept = false });

        if (result.IsSuccess)
            await LoadDataAsync();
        else
            ShowApiError(result.ErrorCode);
    }

    // ── Remove friend ───────────────────────────────────────

    [RelayCommand]
    private async Task RemoveFriendAsync(FriendDto friend)
    {
        var confirm = _dialog.ShowMessage(
            LocalizationManager.Instance.Format("Friends_Message_RemoveFriend", friend.Username),
            LocalizationManager.Instance["Global_Title_Confirm"],
            DialogButton.YesNo, DialogIcon.Question);

        if (confirm != Services.DialogResult.Yes) return;

        var result = await _api.DeleteAsync($"api/social/friends/{friend.UserId}");
        if (result.IsSuccess)
            await LoadDataAsync();
        else
            ShowApiError(result.ErrorCode);
    }

    // ── Navigation ──────────────────────────────────────────

    [RelayCommand]
    private void GoBack() => _navigation.GoBack();

    private void ShowApiError(string? errorCode)
    {
        _dialog.ShowMessage(
            ErrorResolver.Resolve(errorCode),
            LocalizationManager.Instance["Global_Title_Error"],
            DialogButton.OK, DialogIcon.Error);
    }
}
