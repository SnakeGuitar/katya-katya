using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KatyaKatya.Helpers;
using KatyaKatya.Services.Core;
using KatyaKatya.Services.Interfaces;

namespace KatyaKatya.ViewModels.Settings;

public sealed record LanguageOption(string Code, string NativeName)
{
    public override string ToString() => NativeName;
}

/// <summary>
/// Settings view model — music, theme, language, and fullscreen toggle.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private readonly ClientSettings _settings;
    private readonly IMusicService _music;
    private readonly IWindowService _window;
    private readonly IThemeService _theme;

    public static IReadOnlyList<LanguageOption> Languages { get; } =
    [
        new("en-US", "English"),
        new("es-MX", "Español (México)"),
        new("ja-JP", "日本語"),
        new("zh-CN", "中文（简体）"),
        new("ko-KR", "한국어"),
    ];

    public static IReadOnlyList<string> Themes { get; } = ["Pastel", "Sketch"];

    [ObservableProperty] private LanguageOption _selectedLanguage;
    [ObservableProperty] private bool _languageChanged;
    [ObservableProperty] private bool _isMusicEnabled;
    [ObservableProperty] private double _musicVolume;
    [ObservableProperty] private int _selectedTrackIndex;
    [ObservableProperty] private bool _isFullscreen;
    [ObservableProperty] private string _selectedTheme;

    public SettingsViewModel(
        INavigationService navigation,
        ClientSettings settings,
        IMusicService music,
        IWindowService window,
        IThemeService theme)
    {
        _navigation = navigation;
        _settings = settings;
        _music = music;
        _window = window;
        _theme = theme;

        _selectedLanguage = Languages.FirstOrDefault(l => l.Code == settings.LanguageCode) ?? Languages[0];
        _isMusicEnabled = music.IsEnabled;
        _musicVolume = music.Volume;
        _selectedTrackIndex = music.CurrentTrackIndex;
        _isFullscreen = window.IsFullscreen;
        _selectedTheme = settings.ThemeName;
    }

    public IReadOnlyList<string> AvailableTracks => _music.Tracks;
    public string LogoPath => LogoResolver.Resolve(_settings.LanguageCode, _settings.ThemeName);
    public string SettingsTitle => T("Settings", "Configuracion");
    public string LanguageLabel => T("Language", "Idioma");
    public string RestartLanguageLabel => T("* Restart to apply language change", "* Reinicia para aplicar el idioma");
    public string ThemeLabel => T("Theme", "Tema");
    public string MusicLabel => T("Music", "Musica");
    public string FullscreenLabel => T("Fullscreen", "Pantalla completa");
    public string BackLabel => T("< Back", "< Volver");

    partial void OnSelectedLanguageChanged(LanguageOption value)
    {
        _settings.LanguageCode = value.Code;
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(value.Code);
        LanguageChanged = true;
        NotifyLocalizedProperties();
    }

    partial void OnIsMusicEnabledChanged(bool value) => _music.IsEnabled = value;
    partial void OnMusicVolumeChanged(double value) => _music.Volume = value;
    partial void OnSelectedTrackIndexChanged(int value) => _music.CurrentTrackIndex = value;
    partial void OnIsFullscreenChanged(bool value) => _window.SetFullscreen(value);
    
    partial void OnSelectedThemeChanged(string value)
    {
        _settings.ThemeName = value;
        _theme.ApplyTheme(value);
        OnPropertyChanged(nameof(LogoPath));
    }

    private string T(string en, string es)
        => _settings.LanguageCode.StartsWith("es", StringComparison.OrdinalIgnoreCase) ? es : en;

    private void NotifyLocalizedProperties()
    {
        OnPropertyChanged(nameof(LogoPath));
        OnPropertyChanged(nameof(SettingsTitle));
        OnPropertyChanged(nameof(LanguageLabel));
        OnPropertyChanged(nameof(RestartLanguageLabel));
        OnPropertyChanged(nameof(ThemeLabel));
        OnPropertyChanged(nameof(MusicLabel));
        OnPropertyChanged(nameof(FullscreenLabel));
        OnPropertyChanged(nameof(BackLabel));
    }

    [RelayCommand]
    private void GoBack() => _navigation.GoBack();
}
