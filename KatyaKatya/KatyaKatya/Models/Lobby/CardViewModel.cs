using System;
using System.Collections.Generic;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;

namespace KatyaKatya.Models.Lobby;

/// <summary>
/// Observable model for a single card on the multiplayer/singleplayer game board.
/// Tracks face-up/matched state and exposes the current display image.
/// </summary>
public partial class CardViewModel : ObservableObject
{
    private const int DecodeWidth = 256;

    private static readonly Bitmap BackImage = Load(
        "avares://KatyaKatya/Resources/Images/Icons/love-points.png");

    private static readonly Dictionary<string, Bitmap> FrontCache = new();

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
            : BackImage;

    public CardViewModel(int index)
    {
        Index = index;
    }

    private static Bitmap GetFront(string id)
    {
        if (FrontCache.TryGetValue(id, out var cached))
            return cached;

        var img = Load($"avares://KatyaKatya/Resources/Images/Cards/{id}.png");
        FrontCache[id] = img;
        return img;
    }

    private static Bitmap Load(string uri)
    {
        try
        {
            using var stream = AssetLoader.Open(new Uri(uri));
            return Bitmap.DecodeToWidth(stream, DecodeWidth);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CardViewModel] Error loading card bitmap {uri}: {ex.Message}");
            return new WriteableBitmap(new Avalonia.PixelSize(1, 1), new Avalonia.Vector(96, 96), Avalonia.Platform.PixelFormat.Bgra8888, Avalonia.Platform.AlphaFormat.Premul);
        }
    }
}
