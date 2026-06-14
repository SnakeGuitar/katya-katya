using System.IO;
using System.Text.Json;
using KatyaKatya.Engine.Settings;

namespace KatyaKatya.Services.Core;

/// <summary>
/// Persists user preferences to a JSON file in AppData.
/// </summary>
public class ClientSettings
{
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "KatyaKatya",
        "settings.json");

    private SettingsData _data = new();

    public string LanguageCode
    {
        get => _data.LanguageCode;
        set { _data.LanguageCode = value; Save(); }
    }

    public bool MusicEnabled
    {
        get => _data.MusicEnabled;
        set { _data.MusicEnabled = value; Save(); }
    }

    public double MusicVolume
    {
        get => _data.MusicVolume;
        set { _data.MusicVolume = value; Save(); }
    }

    public string ThemeName
    {
        get => _data.ThemeName;
        set { _data.ThemeName = value; Save(); }
    }

    public GraphicsPreset GraphicsPreset
    {
        get => _data.GraphicsPreset;
        set { _data.GraphicsPreset = value; Save(); }
    }

    public bool EnableAnimatedBackground
    {
        get => _data.EnableAnimatedBackground;
        set { _data.EnableAnimatedBackground = value; Save(); }
    }

    public bool EnableParticles
    {
        get => _data.EnableParticles;
        set { _data.EnableParticles = value; Save(); }
    }

    public bool EnableGlassMotion
    {
        get => _data.EnableGlassMotion;
        set { _data.EnableGlassMotion = value; Save(); }
    }

    public bool EnableUiSfx
    {
        get => _data.EnableUiSfx;
        set { _data.EnableUiSfx = value; Save(); }
    }

    public ClientSettings() => Load();

    private void Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath);
                _data = JsonSerializer.Deserialize<SettingsData>(json) ?? new SettingsData();
            }
        }
        catch { _data = new SettingsData(); }
    }

    private void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(_data));
        }
        catch { /* non-critical */ }
    }

    private sealed class SettingsData
    {
        public string LanguageCode { get; set; } = System.Globalization.CultureInfo.CurrentUICulture.Name.StartsWith("es", StringComparison.OrdinalIgnoreCase) ? "es-MX" : "en-US";
        public bool MusicEnabled { get; set; } = true;
        public double MusicVolume { get; set; } = 0.5;
        public string ThemeName { get; set; } = "Pastel";
        public GraphicsPreset GraphicsPreset { get; set; } = GraphicsPreset.Normal;
        public bool EnableAnimatedBackground { get; set; } = true;
        public bool EnableParticles { get; set; } = true;
        public bool EnableGlassMotion { get; set; } = true;
        public bool EnableUiSfx { get; set; } = true;
    }
}
