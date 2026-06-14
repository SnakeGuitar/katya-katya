using System;
using System.Collections.Generic;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using KatyaKatya.Engine.Assets;

namespace KatyaKatya.Models.Lobby;

/// <summary>
/// Observable model for a single card on the multiplayer/singleplayer game board.
/// Tracks face-up/matched state and exposes the current display image.
/// </summary>
public partial class CardViewModel : ObservableObject
{
    private const int DecodeWidth = 256;

    /// <summary>
    /// The card back, decoded once to display width and shared by every card. Binding all
    /// backs to this single small bitmap avoids 30+ full-resolution (1060x1484) decodes of
    /// card-reverse.png, which made the board re-rasterize at ~55 ms/frame.
    /// </summary>
    public static Bitmap CardBack { get; } = Load(
        "avares://KatyaKatya/Resources/Images/Cards/card-reverse.png");

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

    public Bitmap DisplayImage =>
        (IsFlipped || IsMatched) && ImageIdentifier is not null
            ? GetFront(ImageIdentifier)
            : CardBack;

    public CardViewModel(int index)
    {
        Index = index;
    }

    private static Bitmap GetFront(string id)
        => Load($"avares://KatyaKatya/Resources/Images/Cards/{id}.png");

    public static void PreloadFronts(IEnumerable<string> ids)
        => AssetStore.PreloadBitmaps(
            ids.Distinct().Select(id => new VisualAssetId($"avares://KatyaKatya/Resources/Images/Cards/{id}.png")),
            AssetLoadOptions.Width(DecodeWidth));

    private static Bitmap Load(string uri)
    {
        return AssetStore.GetBitmap(new VisualAssetId(uri), AssetLoadOptions.Width(DecodeWidth));
    }

    private static IVisualAssetStore AssetStore =>
        App.Services?.GetService<IVisualAssetStore>()
        ?? throw new InvalidOperationException("VisualAssetStore is not available.");
}
