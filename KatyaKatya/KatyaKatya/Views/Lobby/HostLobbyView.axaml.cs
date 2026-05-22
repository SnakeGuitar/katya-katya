using System;
using Avalonia.Controls;
using KatyaKatya.ViewModels.Lobby;

namespace KatyaKatya.Views.Lobby;

public partial class HostLobbyView : UserControl
{
    private HostLobbyViewModel? _viewModel;

    public HostLobbyView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_viewModel is not null)
        {
            _viewModel.ScrollChatToBottom -= ScrollChat;
        }

        _viewModel = DataContext as HostLobbyViewModel;

        if (_viewModel is not null)
        {
            _viewModel.ScrollChatToBottom += ScrollChat;
        }
    }

    private void ScrollChat()
    {
        var scrollViewer = this.FindControl<ScrollViewer>("ChatScrollViewer");
        scrollViewer?.ScrollToEnd();
    }
}
