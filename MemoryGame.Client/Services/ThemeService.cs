using System.Windows;

namespace MemoryGame.Client.Services;

/// <summary>
/// Swaps the active UI theme at runtime by replacing the first merged
/// ResourceDictionary in Application.Resources with the chosen theme file.
/// Supports "Pastel" (default) and "Sketch".
/// </summary>
public class ThemeService
{
    private static readonly Dictionary<string, Uri> ThemeUris = new()
    {
        ["Pastel"] = new Uri("Resources/Themes/BaseTheme.xaml",  UriKind.Relative),
        ["Sketch"] = new Uri("Resources/Themes/SketchTheme.xaml", UriKind.Relative),
    };

    private readonly ClientSettings _settings;

    public ThemeService(ClientSettings settings)
    {
        _settings = settings;
    }

    /// <summary>Applies the theme stored in <see cref="ClientSettings.ThemeName"/>.</summary>
    public void ApplyStoredTheme() => Apply(_settings.ThemeName);

    /// <summary>Applies a theme by name and persists the choice.</summary>
    public void Apply(string themeName)
    {
        if (!ThemeUris.TryGetValue(themeName, out var uri))
            return;

        var mergedDicts = Application.Current.Resources.MergedDictionaries;

        if (mergedDicts.Count > 0)
            mergedDicts[0] = new ResourceDictionary { Source = uri };
        else
            mergedDicts.Add(new ResourceDictionary { Source = uri });

        _settings.ThemeName = themeName;
    }

    /// <summary>Available theme names.</summary>
    public static IReadOnlyList<string> Themes { get; } = [.. ThemeUris.Keys];
}
