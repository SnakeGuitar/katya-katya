using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MemoryGame.Client.ViewModels.Gallery;

/// <summary>A single variant of a gallery card (one image + label).</summary>
/// <param name="Label">Display label shown below the card image.</param>
/// <param name="ImagePath">Pack-relative path to the image resource.</param>
public record CardVariant(string Label, string ImagePath);

/// <summary>
/// Represents one card in the gallery. Holds all its variants and cycles
/// through them on each <see cref="CycleCommand"/> invocation.
/// </summary>
public partial class GalleryCardViewModel : ObservableObject
{
    private static readonly Random Rng = new();

    private readonly IReadOnlyList<CardVariant> _variants;
    private int _index;

    [ObservableProperty]
    private CardVariant _current;

    /// <summary>Slight random tilt applied to the card tile (-4° to +4°).</summary>
    public double Rotation { get; } = Rng.NextDouble() * 8 - 4;

    public GalleryCardViewModel(IReadOnlyList<CardVariant> variants)
    {
        _variants = variants;
        _current  = variants[0];
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
