using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using MemoryGame.Client.Messages;
using MemoryGame.Client.Services.Core;

namespace MemoryGame.Client.Services.UI;

/// <summary>
/// Swaps the active UI theme at runtime by replacing the first merged
/// ResourceDictionary in Application.Resources with the chosen theme file.
/// Also exposes the active <see cref="IThemeAssets"/> so consumers don't need to
/// branch on the theme name themselves.
/// </summary>
public class ThemeService
{
    private static readonly Dictionary<string, Uri> ThemeUris = new()
    {
        [ThemeIds.Pastel] = new Uri("Resources/Themes/BaseTheme.xaml",   UriKind.Relative),
        [ThemeIds.Sketch] = new Uri("Resources/Themes/SketchTheme.xaml", UriKind.Relative),
    };

    private static readonly Dictionary<string, IThemeAssets> Assets = new()
    {
        [ThemeIds.Pastel] = new PastelThemeAssets(),
        [ThemeIds.Sketch] = new SketchThemeAssets(),
    };

    private readonly ClientSettings _settings;

    public ThemeService(ClientSettings settings)
    {
        _settings = settings;
        Current = ThemeUris.ContainsKey(settings.ThemeName) ? settings.ThemeName : ThemeIds.Pastel;
    }

    /// <summary>The active theme id. Updated by <see cref="Apply"/>.</summary>
    public static string Current { get; private set; } = ThemeIds.Pastel;

    /// <summary>The asset registry for the active theme.</summary>
    public static IThemeAssets CurrentAssets => Assets[Current];

    /// <summary>Available theme names.</summary>
    public static IReadOnlyList<string> Themes { get; } = [.. ThemeUris.Keys];

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

        Current = themeName;
        _settings.ThemeName = themeName;
        WeakReferenceMessenger.Default.Send(new ThemeChangedMessage(themeName));
    }
}
