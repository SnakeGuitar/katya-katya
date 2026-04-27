using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemoryGame.Client.Services.Core;
using MemoryGame.Client.Services.Interfaces;
using MemoryGame.Client.Services.Media;
using MemoryGame.Client.Services.Network;
using MemoryGame.Client.Services.UI;

namespace MemoryGame.Client.ViewModels.Gallery;

/// <summary>
/// Gallery screen. Holds all available cards and their variants.
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
        static GalleryCardViewModel Card(params CardVariant[] variants) => new(variants);

        const string Cards = "/Resources/Images/Cards";
        const string Moods = "/Resources/Images/Backgrounds/katya-moods";

        return
        [
            Card(
                new CardVariant("Color",    $"{Cards}/katya-1/katya-1-no-background.png"),
                new CardVariant("Original", $"{Cards}/katya-1/katya-1-original.png")),

            Card(
                new CardVariant("Color",    $"{Moods}/main/katya-main-no-background.png"),
                new CardVariant("Original", $"{Moods}/main/katya-main.png"),
                new CardVariant("Sketch",   $"{Moods}/main/sketch-katya-main-no-background.png")),

            Card(
                new CardVariant("Color",    $"{Moods}/in-love/katya-in-love-no-background.png"),
                new CardVariant("Original", $"{Moods}/in-love/katya-in-love.png"),
                new CardVariant("Sketch",   $"{Moods}/in-love/sketch-katya-in-love-no-background.png")),

            Card(
                new CardVariant("Color",    $"{Moods}/shy/katya-shy-2-no-background.png"),
                new CardVariant("Original", $"{Moods}/shy/katya-shy-3.png"),
                new CardVariant("Sketch",   $"{Moods}/shy/sketch-katya-shy-no-background.png")),

            Card(
                new CardVariant("Sketch",   $"{Moods}/standing/sketch-katya-standing-no-background.png")),

            Card(
                new CardVariant("Original", $"{Cards}/yumiko-1/yumiko-1-original.png")),

            Card(
                new CardVariant("Original", $"{Cards}/akari-1/akari-1-original.png")),
        ];
    }
}
