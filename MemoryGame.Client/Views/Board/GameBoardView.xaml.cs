using System.Windows.Controls;

namespace MemoryGame.Client.Views.Board;

/// <summary>
/// Code-behind for the multiplayer game board view.
/// Handles chat scroll-to-bottom via ViewModel event.
/// </summary>
public partial class GameBoardView : UserControl
{
    public GameBoardView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is ViewModels.Lobby.GameBoardViewModel oldVm)
            oldVm.ScrollChatToBottom -= ScrollChatToEnd;

        if (e.NewValue is ViewModels.Lobby.GameBoardViewModel newVm)
            newVm.ScrollChatToBottom += ScrollChatToEnd;
    }

    private void ScrollChatToEnd()
    {
        ChatScrollViewer?.ScrollToEnd();
    }
}
