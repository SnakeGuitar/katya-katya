using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using MemoryGame.Client.Engine.Animations;
using MemoryGame.Client.ViewModels.SinglePlayer;

namespace MemoryGame.Client.Views.SinglePlayer;

public partial class SinglePlayerGameView : UserControl
{
    private GameAnimationService? _animations;

    public SinglePlayerGameView()
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
        if (e.OldValue is SinglePlayerGameViewModel oldVm)
        {
            oldVm.PairMatched     -= OnPairMatched;
            oldVm.PropertyChanged -= OnVmPropertyChanged;
        }

        if (e.NewValue is SinglePlayerGameViewModel newVm)
        {
            newVm.PairMatched     += OnPairMatched;
            newVm.PropertyChanged += OnVmPropertyChanged;

            _animations = new GameAnimationService(ParticleCanvas, () =>
                StatsPanel.TranslatePoint(
                    new Point(StatsPanel.ActualWidth / 2, StatsPanel.ActualHeight * 0.45),
                    ParticleCanvas));
        }
    }

    private void OnPairMatched() => _animations?.SpawnParticles();

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SinglePlayerGameViewModel.ShowGameOver)
            && sender is SinglePlayerGameViewModel { ShowGameOver: true })
        {
            GameAnimationService.PlayGameOver(GameOverOverlay, GameOverCard);
        }
    }
}
