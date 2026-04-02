using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemoryGame.Client.Localization;
using MemoryGame.Client.Services;
namespace MemoryGame.Client.ViewModels.Settings;

/// <summary>Represents a language choice in the settings combobox.</summary>
public sealed record LanguageOption(string Code, string NativeName)
{
    public override string ToString() => NativeName;
}

public partial class SettingsViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private readonly ClientSettings     _settings;
    private readonly MusicService       _music;
    private readonly IWindowService     _window;
    private readonly ThemeService       _theme;

    // Always shown in native script so the user can find their language regardless of current UI language.
    public static IReadOnlyList<LanguageOption> Languages { get; } =
    [
        new("en-US", "English"),
        new("es-MX", "Español (México)"),
        new("ja-JP", "日本語"),
        new("zh-CN", "中文（简体）"),
        new("ko-KR", "한국어"),
    ];

    [ObservableProperty] private LanguageOption _selectedLanguage;
    [ObservableProperty] private bool   _isMusicEnabled;
    [ObservableProperty] private double _musicVolume;
    [ObservableProperty] private int    _selectedTrackIndex;
    [ObservableProperty] private bool   _isFullscreen;
    [ObservableProperty] private string _selectedTheme;

    public static IReadOnlyList<string> Themes => ThemeService.Themes;

    public SettingsViewModel(
        INavigationService navigation,
        ClientSettings     settings,
        MusicService       music,
        IWindowService     window,
        ThemeService       theme)
    {
        _navigation = navigation;
        _settings   = settings;
        _music      = music;
        _window     = window;
        _theme      = theme;

        _selectedLanguage   = Languages.FirstOrDefault(l => l.Code == settings.LanguageCode) ?? Languages[0];
        _isMusicEnabled     = settings.MusicEnabled;
        _musicVolume        = settings.MusicVolume;
        _selectedTrackIndex = music.CurrentTrackIndex;
        _isFullscreen       = window.IsFullscreen;
        _selectedTheme      = settings.ThemeName;
    }

    /// <summary>Get the list of available music tracks.</summary>
    public IReadOnlyList<string> AvailableTracks => _music.Tracks;

    // Apply changes immediately — no "Save" button needed.

    partial void OnSelectedLanguageChanged(LanguageOption value)
    {
        _settings.LanguageCode = value.Code;
        LocalizationManager.Instance.SetCulture(value.Code);
    }

    partial void OnIsMusicEnabledChanged(bool value)     => _music.IsEnabled = value;
    partial void OnMusicVolumeChanged(double value)      => _music.Volume = value;
    partial void OnSelectedTrackIndexChanged(int value)  => _music.CurrentTrackIndex = value;
    partial void OnIsFullscreenChanged(bool value)       => _window.SetFullscreen(value);
    partial void OnSelectedThemeChanged(string value)    => _theme.Apply(value);

    [RelayCommand]
    private void GoBack() => _navigation.GoBack();
}
