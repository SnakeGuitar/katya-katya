using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using KatyaKatya.ViewModels.Lobby;

namespace KatyaKatya.Views.Board;

/// <summary>
/// Code-behind for the multiplayer game board view.
/// Handles chat scroll and Game Over animations.
/// Particle effects (GameAnimationService) planned for future SkiaSharp integration.
/// </summary>
public partial class GameBoardView : UserControl
{
    public GameBoardView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        var oldVm = (DataContext as GameBoardViewModel);
        if (oldVm != null)
        {
            oldVm.ScrollChatToBottom -= ScrollChatToEnd;
            oldVm.PropertyChanged -= OnVmPropertyChanged;
        }

        var newVm = DataContext as GameBoardViewModel;
        if (newVm != null)
        {
            newVm.ScrollChatToBottom += ScrollChatToEnd;
            newVm.PropertyChanged += OnVmPropertyChanged;
        }
    }

    private void ScrollChatToEnd()
    {
        var scrollViewer = this.FindControl<ScrollViewer>("ChatScrollViewer");
        scrollViewer?.ScrollToEnd();
    }

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GameBoardViewModel.ShowGameOver)
            && sender is GameBoardViewModel { ShowGameOver: true })
        {
            // Game Over animations are driven by GameAnimations.axaml keyframe animations
            // triggered when ShowGameOver binding changes to true and the backdrop becomes visible
        }
    }
}
