using Avalonia.Controls;
using KatyaKatya.ViewModels.Lobby;

namespace KatyaKatya.Views.Lobby;

public partial class LobbyView : UserControl
{
    private LobbyViewModel? _viewModel;

    public LobbyView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        DetachedFromVisualTree += (_, _) => SetViewModel(null);
    }

    private void OnDataContextChanged(object? sender, EventArgs e) =>
        SetViewModel(DataContext as LobbyViewModel);

    private void SetViewModel(LobbyViewModel? viewModel)
    {
        if (_viewModel is not null)
            _viewModel.ScrollChatToBottom -= ScrollChat;

        _viewModel = viewModel;

        if (_viewModel is not null)
            _viewModel.ScrollChatToBottom += ScrollChat;
    }

    private void ScrollChat() => ChatScrollViewer?.ScrollToEnd();
}
