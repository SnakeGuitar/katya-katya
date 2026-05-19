using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MemoryGame.Client.Models.Lobby;

/// <summary>
/// Observable model for a single card on the multiplayer game board.
/// Tracks face-up/matched state and exposes the current display image.
/// </summary>
public partial class CardViewModel : ObservableObject
{
    private const int DecodeWidth = 256;

    private static readonly ImageSource BackImage = Load(
        "pack://application:,,,/Resources/Images/Icons/love-points.png");

    private static readonly Dictionary<string, ImageSource> FrontCache = new();

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

    public ImageSource DisplayImage =>
        (IsFlipped || IsMatched) && ImageIdentifier is not null
            ? GetFront(ImageIdentifier)
            : BackImage;

    public CardViewModel(int index)
    {
        Index = index;
    }

    private static ImageSource GetFront(string id)
    {
        if (FrontCache.TryGetValue(id, out var cached))
            return cached;

        var img = Load($"pack://application:,,,/Resources/Images/Cards/{id}.png");
        FrontCache[id] = img;
        return img;
    }

    // Decode once at a bounded width and freeze so all cards share the surface.
    private static ImageSource Load(string uri)
    {
        var bmp = new BitmapImage();
        bmp.BeginInit();
        bmp.CacheOption       = BitmapCacheOption.OnLoad;
        bmp.DecodePixelWidth  = DecodeWidth;
        bmp.UriSource         = new Uri(uri);
        bmp.EndInit();
        bmp.Freeze();
        return bmp;
    }
}
