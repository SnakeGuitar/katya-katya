using Avalonia.Controls;
using System.ComponentModel;
using KatyaKatya.ViewModels.Lobby;

namespace KatyaKatya.Views.Lobby;

public partial class GameBoardView : UserControl
{
    private GameBoardViewModel? _viewModel;

    public GameBoardView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        DetachedFromVisualTree += (_, _) => SetViewModel(null);
    }

    private void OnDataContextChanged(object? sender, EventArgs e) =>
        SetViewModel(DataContext as GameBoardViewModel);

    private void SetViewModel(GameBoardViewModel? viewModel)
    {
        if (_viewModel is not null)
        {
            _viewModel.ScrollChatToBottom -= ScrollChat;
            _viewModel.PairMatched -= OnPairMatched;
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        _viewModel = viewModel;

        if (_viewModel is not null)
        {
            _viewModel.ScrollChatToBottom += ScrollChat;
            _viewModel.PairMatched += OnPairMatched;
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            ParticleCanvas.Start();
        }
        else
        {
            ParticleCanvas.Stop();
        }
    }

    private void ScrollChat() => ChatScrollViewer?.ScrollToEnd();

    private void OnPairMatched() =>
        ParticleCanvas.SpawnMatchBurst(new Avalonia.Point(
            ParticleCanvas.Bounds.Width * 0.37,
            ParticleCanvas.Bounds.Height * 0.5));

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GameBoardViewModel.ShowGameOver)
            && sender is GameBoardViewModel { ShowGameOver: true })
        {
            ParticleCanvas.PlayGameOver();
        }
    }
}
