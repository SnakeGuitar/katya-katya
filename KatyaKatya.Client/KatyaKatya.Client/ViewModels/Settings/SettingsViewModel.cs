using System.Collections.Generic;
using System.Linq;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KatyaKatya.Helpers;
using KatyaKatya.Localization;
using KatyaKatya.Services.Core;
using KatyaKatya.Services.Interfaces;

namespace KatyaKatya.ViewModels.Settings;

public sealed record LanguageOption(string Code, string NativeName)
{
    public override string ToString() => NativeName;
}

/// <summary>
/// Settings view model: music, theme, language, and fullscreen toggle.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private readonly ClientSettings _settings;
    private readonly IMusicService _music;
    private readonly IWindowService _window;
    private readonly IThemeService _theme;

    // Always shown in native script so the user can find their language regardless of current UI language.
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

        _music.TracksChanged += OnTracksChanged;
    }

    public IReadOnlyList<string> AvailableTracks => _music.Tracks;
    public string LogoPath => LogoResolver.Resolve(_settings.LanguageCode, _settings.ThemeName);
    public string BackgroundPath => ThemeAssets.GetGlobalBackgroundPath(_settings.ThemeName);
    public string SettingsTitle => LocalizationManager.Instance["Settings_Label_Language"];
    public string LanguageLabel => LocalizationManager.Instance["Settings_Label_Language"];
    public string ThemeLabel => LocalizationManager.Instance.TryGet("Settings_Label_Theme") ?? "Theme";
    public string MusicLabel => LocalizationManager.Instance["Settings_Label_Music"];
    public string TrackLabel => LocalizationManager.Instance.TryGet("Settings_Label_Track") ?? "Track";
    public string FullscreenLabel => LocalizationManager.Instance["Settings_Label_Fullscreen"];
    public string BackLabel => $"< {LocalizationManager.Instance["Global_Button_Back"]}";

    partial void OnSelectedLanguageChanged(LanguageOption value)
    {
        _settings.LanguageCode = value.Code;
        LocalizationManager.Instance.SetCulture(value.Code);
        NotifyLocalizedProperties();
    }

    partial void OnIsMusicEnabledChanged(bool value) => _music.IsEnabled = value;
    partial void OnMusicVolumeChanged(double value) => _music.Volume = value;
    partial void OnSelectedTrackIndexChanged(int value)
    {
        if (value >= 0)
            _music.CurrentTrackIndex = value;
    }
    partial void OnIsFullscreenChanged(bool value) => _window.SetFullscreen(value);

    partial void OnSelectedThemeChanged(string value)
    {
        _settings.ThemeName = value;
        _theme.ApplyTheme(value);
        OnPropertyChanged(nameof(LogoPath));
        OnPropertyChanged(nameof(BackgroundPath));
    }

    private void OnTracksChanged() =>
        Dispatcher.UIThread.Post(() =>
        {
            OnPropertyChanged(nameof(AvailableTracks));
            if (SelectedTrackIndex < 0 && AvailableTracks.Count > 0)
                SelectedTrackIndex = 0;
        });

    private void NotifyLocalizedProperties()
    {
        OnPropertyChanged(nameof(LogoPath));
        OnPropertyChanged(nameof(SettingsTitle));
        OnPropertyChanged(nameof(LanguageLabel));
        OnPropertyChanged(nameof(ThemeLabel));
        OnPropertyChanged(nameof(MusicLabel));
        OnPropertyChanged(nameof(TrackLabel));
        OnPropertyChanged(nameof(FullscreenLabel));
        OnPropertyChanged(nameof(BackLabel));
    }

    [RelayCommand]
    private void GoBack() => _navigation.GoBack();
}
