using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemoryGame.Client.Services.Interfaces;

namespace MemoryGame.Client.ViewModels.Gallery;

/// <summary>
/// Gallery screen. Holds all available cards laid out as scattered stickers.
/// Add new cards to <see cref="BuildCards"/> as assets are introduced.
/// </summary>
public partial class GalleryViewModel : ObservableObject
{
    private readonly INavigationService _navigation;

    public IReadOnlyList<GalleryCardViewModel> Cards { get; }

    /// <summary>The card currently shown in the expanded overlay, or null when closed.</summary>
    [ObservableProperty]
    private GalleryCardViewModel? _selectedCard;

    public GalleryViewModel(INavigationService navigation)
    {
        _navigation = navigation;
        Cards       = BuildCards();
    }

    /// <summary>Opens the expansion overlay for the given card.</summary>
    [RelayCommand]
    private void SelectCard(GalleryCardViewModel card) => SelectedCard = card;

    /// <summary>Closes the expansion overlay.</summary>
    [RelayCommand]
    private void CloseCard() => SelectedCard = null;

    [RelayCommand]
    private void GoBack() => _navigation.GoBack();

    // ── card catalogue ───────────────────────────────────────────────────────

    private static IReadOnlyList<GalleryCardViewModel> BuildCards()
    {
        const string Cards = "/Resources/Images/Cards";
        const string Moods = "/Resources/Images/Backgrounds/katya-moods";

        // Variants per card. First entry is shown by default.
        var sets = new CardVariant[][]
        {
            [
                new("Color",  $"{Cards}/katya-1/katya-1-no-background.png"),
                new("Sketch", $"{Cards}/katya-1/katya-1-original-border.png"),
            ],
            [
                new("Color",  $"{Moods}/main/katya-main-no-background.png"),
                new("Sketch", $"{Moods}/main/sketch-katya-main-no-background.png"),
            ],
            [
                new("Color",  $"{Moods}/in-love/katya-in-love-no-background.png"),
                new("Sketch", $"{Moods}/in-love/sketch-katya-in-love-no-background.png"),
            ],
            [
                new("Color",  $"{Moods}/shy/katya-shy-2-no-background.png"),
                new("Sketch", $"{Moods}/shy/sketch-katya-shy-no-background.png"),
            ],
            [
                new("Sketch", $"{Moods}/standing/sketch-katya-standing-no-background.png"),
            ],
            [
                new("Sketch", $"{Cards}/yumiko-1/yumiko-1-original.png"),
            ],
            [
                new("Sketch", $"{Cards}/akari-1/akari-1-original.png"),
            ],
            [
                new("Sitting", "/Resources/Images/katya-sit-down.png"),
            ],
        };

        return ScatterLayout(sets);
    }

    /// <summary>
    /// Places the cards over a 4×2 grid with random jitter, rotation, size,
    /// and z-index so they look like stickers piled on a market stand.
    /// Uses a fixed seed so the layout is stable between sessions.
    /// </summary>
    private static IReadOnlyList<GalleryCardViewModel> ScatterLayout(CardVariant[][] sets)
    {
        const int    cols      = 4;
        const double slotW     = 290;
        const double slotH     = 420;
        const double baseX     = 30;
        const double baseY     = 20;
        const double jitterXY  = 40;
        const double tiltDeg   = 14;

        var rng    = new Random(0xC4F5);
        var result = new GalleryCardViewModel[sets.Length];

        for (int i = 0; i < sets.Length; i++)
        {
            int    col      = i % cols;
            int    row      = i / cols;
            double maxHeight = 340 + rng.NextDouble() * 40;       // 340–380
            double maxWidth  = 280;
            double x        = baseX + col * slotW + (rng.NextDouble() - 0.5) * jitterXY;
            double y        = baseY + row * slotH + (rng.NextDouble() - 0.5) * jitterXY;
            double rotation = (rng.NextDouble() - 0.5) * 2 * tiltDeg;
            int    zIndex   = rng.Next(100);

            result[i] = new GalleryCardViewModel(sets[i], x, y, maxWidth, maxHeight, rotation, zIndex);
        }

        return result;
    }
}
