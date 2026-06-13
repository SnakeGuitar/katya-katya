using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using MemoryGame.Client.Engine.Animations;
using MemoryGame.Client.ViewModels.Lobby;

namespace MemoryGame.Client.Views.Board;

/// <summary>
/// Code-behind for the multiplayer game board view.
/// Handles chat scroll, Game Over animation, and particle effects via <see cref="GameAnimationService"/>.
/// </summary>
public partial class GameBoardView : UserControl
{
    private GameAnimationService? _animations;

    public GameBoardView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Unloaded += OnUnloaded;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animations?.Dispose();
        _animations = null;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is GameBoardViewModel oldVm)
        {
            oldVm.ScrollChatToBottom -= ScrollChatToEnd;
            oldVm.PairMatched        -= OnPairMatched;
            oldVm.PropertyChanged    -= OnVmPropertyChanged;
        }

        if (e.NewValue is GameBoardViewModel newVm)
        {
            newVm.ScrollChatToBottom += ScrollChatToEnd;
            newVm.PairMatched        += OnPairMatched;
            newVm.PropertyChanged    += OnVmPropertyChanged;

            _animations = new GameAnimationService(ParticleCanvas, () =>
                new Point(ParticleCanvas.ActualWidth * 0.37, ParticleCanvas.ActualHeight * 0.5));
        }
    }

    private void ScrollChatToEnd() => ChatScrollViewer?.ScrollToEnd();

    private void OnPairMatched() => _animations?.SpawnParticles();

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GameBoardViewModel.ShowGameOver)
            && sender is GameBoardViewModel { ShowGameOver: true })
        {
            GameAnimationService.PlayGameOver(GameOverOverlay, GameOverCard);
        }
    }
}
