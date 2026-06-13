using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemoryGame.Client.Localization;
using MemoryGame.Client.Models;
using MemoryGame.Client.Services.Core;
using MemoryGame.Client.Services.Interfaces;
using MemoryGame.Client.Services.Media;
using MemoryGame.Client.Services.Network;
using MemoryGame.Client.Services.UI;
using MemoryGame.Client.ViewModels.Common;

namespace MemoryGame.Client.ViewModels.Social;


/// <summary>
/// Friends list + friend requests management.
/// Mirrors the legacy FriendsMenu with two tabs: My Friends / Requests.
/// </summary>
public partial class FriendsViewModel : BaseViewModel
{
    private readonly ISessionService _session;
    private readonly ApiClient _api;

    public ObservableCollection<FriendDto> Friends { get; } = [];
    public ObservableCollection<FriendRequestDto> PendingRequests { get; } = [];

    [ObservableProperty] private string _searchUsername = string.Empty;
    [ObservableProperty] private bool _hasFriends;
    [ObservableProperty] private bool _hasRequests;


    public FriendsViewModel(
        INavigationService navigation,
        ISessionService session,
        ApiClient api,
        IDialogService dialog) : base(navigation, dialog)
    {
        _session = session;
        _api = api;

        _ = LoadDataAsync();
    }


    private Task LoadDataAsync() => RunAsync(async () =>
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
    });


    // ── Send friend request ─────────────────────────────────

    [RelayCommand]
    private async Task SendRequestAsync()
    {
        var username = SearchUsername.Trim();
        if (string.IsNullOrEmpty(username)) return;

        if (username == _session.Current?.Username)
        {
            Dialog.ShowMessage(
                LocalizationManager.Instance["Error_SOCIAL_CANNOT_ADD_SELF"],
                LocalizationManager.Instance["Global_Title_Error"],
                DialogButton.OK, DialogIcon.Error);
            return;
        }

        var result = await _api.PostAsync("api/social/friends/request",
            new { ReceiverUsername = username });

        if (result.IsSuccess)
        {
            Dialog.ShowMessage(
                LocalizationManager.Instance.Format("Friends_Message_RequestSent", username),
                LocalizationManager.Instance["Global_Title_Success"],
                DialogButton.OK, DialogIcon.Information);
            SearchUsername = string.Empty;
        }
        else
        {
            await HandleResponseAsync(result);
        }

    }

    // ── Accept / Reject requests ────────────────────────────

    [RelayCommand]
    private async Task AcceptRequestAsync(int requestId)
    {
        var result = await _api.PostAsync("api/social/friends/request/answer",
            new { RequestId = requestId, Accept = true });

        if (await HandleResponseAsync(result))
            await LoadDataAsync();
    }

    [RelayCommand]
    private async Task RejectRequestAsync(int requestId)
    {
        var result = await _api.PostAsync("api/social/friends/request/answer",
            new { RequestId = requestId, Accept = false });

        if (await HandleResponseAsync(result))
            await LoadDataAsync();
    }


    // ── Remove friend ───────────────────────────────────────

    [RelayCommand]
    private async Task RemoveFriendAsync(FriendDto friend)
    {
        var confirm = Dialog.ShowMessage(
            LocalizationManager.Instance.Format("Friends_Message_RemoveFriend", friend.Username),
            LocalizationManager.Instance["Global_Title_Confirm"],
            DialogButton.YesNo, DialogIcon.Question);

        if (confirm != DialogResult.Yes) return;

        var result = await _api.DeleteAsync($"api/social/friends/{friend.UserId}");
        if (await HandleResponseAsync(result))
            await LoadDataAsync();
    }
}

