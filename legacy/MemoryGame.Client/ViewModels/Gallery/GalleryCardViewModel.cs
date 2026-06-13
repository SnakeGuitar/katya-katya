using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MemoryGame.Client.ViewModels.Gallery;

/// <summary>A single variant of a gallery card (one image + label).</summary>
/// <param name="Label">Display label shown below the card image.</param>
/// <param name="ImagePath">Pack-relative path to the image resource.</param>
public record CardVariant(string Label, string ImagePath);

/// <summary>
/// Represents one card in the gallery. Holds all its variants and cycles
/// through them on each <see cref="CycleCommand"/> invocation. Position, size,
/// and tilt are computed by the parent gallery so cards lay out as scattered
/// stickers on a Canvas.
/// </summary>
public partial class GalleryCardViewModel : ObservableObject
{
    private readonly IReadOnlyList<CardVariant> _variants;
    private int _index;

    [ObservableProperty]
    private CardVariant _current;

    /// <summary>Canvas.Left position.</summary>
    public double X { get; }

    /// <summary>Canvas.Top position.</summary>
    public double Y { get; }

    /// <summary>Maximum render width for the sticker image (Stretch=Uniform fits within this).</summary>
    public double MaxWidth { get; }

    /// <summary>Maximum render height for the sticker image.</summary>
    public double MaxHeight { get; }

    /// <summary>Tilt in degrees applied to the sticker.</summary>
    public double Rotation { get; }

    /// <summary>Initial Z-order so some stickers sit above others.</summary>
    public int ZIndex { get; }

    public GalleryCardViewModel(IReadOnlyList<CardVariant> variants,
                                double x, double y, double maxWidth, double maxHeight,
                                double rotation, int zIndex)
    {
        _variants  = variants;
        _current   = variants[0];
        X          = x;
        Y          = y;
        MaxWidth   = maxWidth;
        MaxHeight  = maxHeight;
        Rotation   = rotation;
        ZIndex     = zIndex;
    }

    /// <summary>Advances to the next variant, wrapping back to the first.</summary>
    [RelayCommand]
    private void Cycle()
    {
        _index  = (_index + 1) % _variants.Count;
        Current = _variants[_index];
    }

    /// <summary>Goes to the previous variant, wrapping to the end.</summary>
    [RelayCommand]
    private void CycleBack()
    {
        _index = _index - 1;
        if (_index < 0) _index = _variants.Count - 1;
        Current = _variants[_index];
    }
}
