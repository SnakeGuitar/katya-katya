using System.Windows.Media;

namespace MemoryGame.Client.Services.UI;

/// <summary>
/// Per-theme content registry. XAML resource dictionaries handle visual styles;
/// this interface centralizes the non-XAML pieces that vary by theme
/// (asset paths, particle palettes, etc.) so consumers don't sprinkle
/// "if (theme == Sketch)" branches across the codebase.
/// </summary>
public interface IThemeAssets
{
    /// <summary>Returns a pack URI for the localized title-screen logo.</summary>
    /// <param name="languagePrefix">Two-letter prefix used by the asset filenames (e.g. "es", "en").</param>
    string LogoPath(string languagePrefix);

    /// <summary>Pack-relative paths to the rotating mood backgrounds shown on the main menu.</summary>
    IReadOnlyList<string> MainMenuMoodImages { get; }

    /// <summary>Colors sampled at random for falling petal particles.</summary>
    IReadOnlyList<Color> PetalColors { get; }
}

internal sealed class PastelThemeAssets : IThemeAssets
{
    public string LogoPath(string languagePrefix) =>
        $"pack://application:,,,/Resources/Images/Logos/logo-{languagePrefix}.png";

    public IReadOnlyList<string> MainMenuMoodImages { get; } =
    [
        "/Resources/Images/Backgrounds/katya-moods/main/katya-main-no-background.png",
        "/Resources/Images/Backgrounds/katya-moods/in-love/katya-in-love-no-background.png",
        "/Resources/Images/Backgrounds/katya-moods/shy/katya-shy-2-no-background.png",
    ];

    public IReadOnlyList<Color> PetalColors { get; } =
    [
        Color.FromArgb(210, 255, 200, 215),
        Color.FromArgb(190, 255, 220, 235),
        Color.FromArgb(220, 240, 175, 200),
        Color.FromArgb(180, 255, 245, 248),
        Color.FromArgb(200, 255, 170, 190),
    ];
}

internal sealed class SketchThemeAssets : IThemeAssets
{
    public string LogoPath(string languagePrefix) =>
        $"pack://application:,,,/Resources/Images/Logos/sketch-logo-{languagePrefix}.png";

    public IReadOnlyList<string> MainMenuMoodImages { get; } =
    [
        "/Resources/Images/Backgrounds/katya-moods/main/sketch-katya-main-no-background.png",
        "/Resources/Images/Backgrounds/katya-moods/in-love/sketch-katya-in-love-no-background.png",
        "/Resources/Images/Backgrounds/katya-moods/shy/sketch-katya-shy-no-background.png",
        "/Resources/Images/Backgrounds/katya-moods/standing/sketch-katya-standing-no-background.png",
    ];

    public IReadOnlyList<Color> PetalColors { get; } =
    [
        Color.FromArgb(190, 235, 235, 235),
        Color.FromArgb(170, 210, 210, 210),
        Color.FromArgb(200, 250, 250, 250),
        Color.FromArgb(160, 180, 180, 180),
        Color.FromArgb(140, 120, 120, 120),
    ];
}
