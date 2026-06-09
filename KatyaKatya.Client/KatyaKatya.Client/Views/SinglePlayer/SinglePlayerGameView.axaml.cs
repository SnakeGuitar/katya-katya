using Avalonia.Controls;
using System.ComponentModel;
using KatyaKatya.ViewModels.SinglePlayer;

namespace KatyaKatya.Views.SinglePlayer;

public partial class SinglePlayerGameView : UserControl
{
    private SinglePlayerGameViewModel? _viewModel;

    public SinglePlayerGameView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        DetachedFromVisualTree += (_, _) => SetViewModel(null);
    }

    private void OnDataContextChanged(object? sender, EventArgs e) =>
        SetViewModel(DataContext as SinglePlayerGameViewModel);

    private void SetViewModel(SinglePlayerGameViewModel? viewModel)
    {
        if (_viewModel is not null)
        {
            _viewModel.PairMatched -= OnPairMatched;
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        _viewModel = viewModel;

        if (_viewModel is not null)
        {
            _viewModel.PairMatched += OnPairMatched;
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            ParticleCanvas.Start();
        }
        else
        {
            ParticleCanvas.Stop();
        }
    }

    private void OnPairMatched()
    {
        ParticleCanvas.SpawnMatchBurst(new Avalonia.Point(
            ParticleCanvas.Bounds.Width / 2,
            ParticleCanvas.Bounds.Height * 0.18));
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SinglePlayerGameViewModel.ShowGameOver)
            && sender is SinglePlayerGameViewModel { ShowGameOver: true })
        {
            ParticleCanvas.PlayGameOver();
        }
    }
}
