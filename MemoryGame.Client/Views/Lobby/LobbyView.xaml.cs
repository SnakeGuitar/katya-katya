using System.Windows.Controls;
using MemoryGame.Client.ViewModels.Lobby;

namespace MemoryGame.Client.Views.Lobby;

public partial class LobbyView : UserControl
{
    public LobbyView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is LobbyViewModel oldVm)
            oldVm.ScrollChatToBottom -= ScrollChat;

        if (e.NewValue is LobbyViewModel newVm)
            newVm.ScrollChatToBottom += ScrollChat;
    }

    private void ScrollChat()
    {
        ChatScrollViewer?.ScrollToEnd();
    }
}
