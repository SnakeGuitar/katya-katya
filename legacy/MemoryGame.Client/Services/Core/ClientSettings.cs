using System.IO;
using System.Text.Json;
using MemoryGame.Client.Services.UI;

namespace MemoryGame.Client.Services.Core;

/// <summary>
/// Persists user preferences to a JSON file in AppData.
/// </summary>
public class ClientSettings
{
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "MemoryGame",
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
        public string LanguageCode { get; set; } = "en-US";
        public bool   MusicEnabled  { get; set; } = true;
        public double MusicVolume   { get; set; } = 0.5;
        public string ThemeName     { get; set; } = ThemeIds.Pastel;
    }
}
