using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemoryGame.Client.Services;

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
        // Helper: build a card entry from a folder that follows the naming convention
        // Cards/<id>/<id>-no-background.png  +  Cards/<id>/<id>-original.png
        static GalleryCardViewModel Card(string id) => new(
        [
            new("Color",    $"/Resources/Images/Cards/{id}/{id}-no-background.png"),
            new("Original", $"/Resources/Images/Cards/{id}/{id}-original.png"),
        ]);

        // TODO: replace placeholder ids with real asset folders as they are added
        return
        [
            Card("katya-1"),
            Card("katya-1"), // africa
            Card("katya-1"), // ana
            Card("katya-1"), // ari
            Card("katya-1"), // blanca
            Card("katya-1"), // emily
            Card("katya-1"), // fer
            Card("katya-1"), // lala
            Card("katya-1"), // linda
            Card("katya-1"), // paul
            Card("katya-1"), // saddy
            Card("katya-1"), // sara
        ];
    }
}
