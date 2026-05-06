using CommunityToolkit.Mvvm.ComponentModel;

namespace MemoryGame.Client.Models.Lobby;

/// <summary>
/// Observable model for a single card on the multiplayer game board.
/// Tracks face-up/matched state and exposes the current display image.
/// </summary>
public partial class CardViewModel : ObservableObject
{
    private static readonly string BackImage = "pack://application:,,,/Resources/Images/Icons/love-points.png";

    public int Index { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayImage))]
    private bool _isFlipped;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayImage))]
    private bool _isMatched;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayImage))]
    private string? _imageIdentifier;

    /// <summary>
    /// The image source to render: front image when flipped/matched, back image otherwise.
    /// In the revival, imageIdentifier maps to a resource path (e.g. "card_000" → "/Resources/Images/Cards/card_000.png").
    /// For now, we show the identifier as-is (the server sends the full relative path).
    /// </summary>
    public string DisplayImage => (IsFlipped || IsMatched) && ImageIdentifier is not null
        ? $"pack://application:,,,/Resources/Images/Cards/{ImageIdentifier}.png"
        : BackImage;

    public CardViewModel(int index)
    {
        Index = index;
    }
}
