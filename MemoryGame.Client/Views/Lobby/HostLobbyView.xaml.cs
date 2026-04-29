using System.Windows.Controls;
using MemoryGame.Client.ViewModels.Lobby;

namespace MemoryGame.Client.Views.Lobby;

public partial class HostLobbyView : UserControl
{
    public HostLobbyView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is HostLobbyViewModel oldVm)
            oldVm.ScrollChatToBottom -= ScrollChat;

        if (e.NewValue is HostLobbyViewModel newVm)
            newVm.ScrollChatToBottom += ScrollChat;
    }

    private void ScrollChat()
    {
        ChatScrollViewer?.ScrollToEnd();
    }
}
