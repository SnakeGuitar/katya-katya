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
        DetachedFromVisualTree += (_, _) => SetViewModel(null);
    }

    private void OnDataContextChanged(object? sender, EventArgs e) =>
        SetViewModel(DataContext as HostLobbyViewModel);

    private void SetViewModel(HostLobbyViewModel? viewModel)
    {
        if (_viewModel is not null)
            _viewModel.ScrollChatToBottom -= ScrollChat;

        _viewModel = viewModel;

        if (_viewModel is not null)
            _viewModel.ScrollChatToBottom += ScrollChat;
    }

    private void ScrollChat() => ChatScrollViewer?.ScrollToEnd();
}
