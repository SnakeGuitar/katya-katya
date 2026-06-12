using Avalonia.Media;

namespace KatyaKatya.Helpers;

public interface IThemeAssetService
{
    string LogoPath(string languageCode, string themeName);
    string GlobalBackgroundPath(string themeName);
    string MainMenuBackgroundPath(string themeName);
    IReadOnlyList<string> MainMenuMoodImages(string themeName);
    IReadOnlyList<Color> ParticleColors(string themeName);
}

public sealed class ThemeAssetService : IThemeAssetService
{
    public string LogoPath(string languageCode, string themeName) =>
        LogoResolver.Resolve(languageCode, themeName);

    public string GlobalBackgroundPath(string themeName) =>
        ThemeAssets.GetGlobalBackgroundPath(themeName);

    public string MainMenuBackgroundPath(string themeName) =>
        ThemeAssets.GetMainMenuBackgroundPath(themeName);

    public IReadOnlyList<string> MainMenuMoodImages(string themeName) =>
        ThemeAssets.GetMainMenuMoodImages(themeName);

    public IReadOnlyList<Color> ParticleColors(string themeName) =>
        ThemeAssets.GetParticleColors(themeName);
}

public static class ThemeAssets
{
    public static string CurrentThemeName { get; private set; } = "Pastel";

    public static event Action? ThemeChanged;

    public static void SetCurrentTheme(string themeName)
    {
        CurrentThemeName = IsSketch(themeName) ? "Sketch" : "Pastel";
        ThemeChanged?.Invoke();
    }

    public static string GetLogoPath(string themeName, string languagePrefix)
    {
        var fileName = IsSketch(themeName)
            ? $"sketch-logo-{languagePrefix}.png"
            : $"logo-{languagePrefix}.png";

        return $"avares://KatyaKatya/Resources/Images/Logos/{fileName}";
    }

    public static string GetGlobalBackgroundPath(string themeName) =>
        IsSketch(themeName)
            ? "avares://KatyaKatya/Resources/Images/Backgrounds/background-sketch.png"
            : "avares://KatyaKatya/Resources/Images/Backgrounds/background-minimalistic.png";

    public static string GetMainMenuBackgroundPath(string themeName) =>
        IsSketch(themeName)
            ? "avares://KatyaKatya/Resources/Images/Backgrounds/background-sketch.png"
            : "avares://KatyaKatya/Resources/Images/Backgrounds/katya-main-background-only.png";

    public static string GetMenuIconPath(string themeName) =>
        IsSketch(themeName)
            ? "avares://KatyaKatya/Resources/Images/Icons/sketch-menu-icon.png"
            : "avares://KatyaKatya/Resources/Images/Icons/menu-icon.png";

    public static IReadOnlyList<string> GetMainMenuMoodImages(string themeName) =>
        IsSketch(themeName) ? SketchMainMenuMoodImages : PastelMainMenuMoodImages;

    public static IReadOnlyList<Color> GetParticleColors(string themeName) =>
        IsSketch(themeName) ? SketchParticleColors : PastelParticleColors;

    private static bool IsSketch(string? themeName) =>
        string.Equals(themeName, "Sketch", StringComparison.OrdinalIgnoreCase);

    private static readonly string[] PastelMainMenuMoodImages =
    [
        "avares://KatyaKatya/Resources/Images/Backgrounds/katya-moods/main/katya-main-no-background.png",
        "avares://KatyaKatya/Resources/Images/Backgrounds/katya-moods/in-love/katya-in-love-no-background.png",
        "avares://KatyaKatya/Resources/Images/Backgrounds/katya-moods/shy/katya-shy-2-no-background.png",
        "avares://KatyaKatya/Resources/Images/Backgrounds/katya-moods/sitting/katya-sit-no-background.png"
    ];

    private static readonly string[] SketchMainMenuMoodImages =
    [
        "avares://KatyaKatya/Resources/Images/Backgrounds/katya-moods/main/sketch-katya-main-no-background.png",
        "avares://KatyaKatya/Resources/Images/Backgrounds/katya-moods/in-love/sketch-katya-in-love-no-background.png",
        "avares://KatyaKatya/Resources/Images/Backgrounds/katya-moods/shy/sketch-katya-shy-no-background.png",
        "avares://KatyaKatya/Resources/Images/Backgrounds/katya-moods/standing/sketch-katya-standing-no-background.png"
    ];

    private static readonly Color[] PastelParticleColors =
    [
        Color.FromArgb(210, 255, 200, 215),
        Color.FromArgb(190, 255, 220, 235),
        Color.FromArgb(220, 240, 175, 200),
        Color.FromArgb(180, 255, 245, 248),
        Color.FromArgb(200, 255, 170, 190)
    ];

    private static readonly Color[] SketchParticleColors =
    [
        Color.FromArgb(190, 235, 235, 235),
        Color.FromArgb(170, 210, 210, 210),
        Color.FromArgb(200, 250, 250, 250),
        Color.FromArgb(160, 180, 180, 180),
        Color.FromArgb(140, 120, 120, 120)
    ];
}
