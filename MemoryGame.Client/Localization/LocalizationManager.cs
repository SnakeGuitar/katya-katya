using System.Globalization;
using System.Resources;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MemoryGame.Client.Localization;

/// <summary>
/// Observable singleton that wraps the .resx ResourceManager.
/// XAML binds to it via the indexer; calling SetCulture() refreshes all bindings.
/// </summary>
public sealed class LocalizationManager : ObservableObject
{
    public static readonly LocalizationManager Instance = new();

    private static readonly ResourceManager ResourceManager = new(
        "MemoryGame.Client.Properties.Langs.Lang",
        Assembly.GetExecutingAssembly());

    private CultureInfo _culture = CultureInfo.GetCultureInfo("en-US");

    private LocalizationManager() { }

    /// <summary>
    /// Returns the localized string for the given key.
    /// Falls back to [key] if not found so missing keys are visible during dev.
    /// </summary>
    public string this[string key]
    {
        get
        {
            try
            {
                return ResourceManager.GetString(key, _culture) ?? $"[{key}]";
            }
            catch
            {
                return $"[{key}]";
            }
        }
    }

    /// <summary>
    /// The currently active culture code (e.g. "en-US", "es-MX").
    /// </summary>
    public string CurrentCultureCode => _culture.Name;

    /// <summary>
    /// Returns the ImageSource for the logo corresponding to the current language.
    /// Returning ImageSource (not string) is required so WPF binding to Image.Source works
    /// at runtime — the string TypeConverter only applies at XAML parse time, not via binding.
    /// </summary>
    public ImageSource LogoImage
    {
        get
        {
            var path = _culture.Name switch
            {
                "es-MX" => "pack://application:,,,/Resources/Images/Logos/logo-es.png",
                "ja-JP" => "pack://application:,,,/Resources/Images/Logos/logo-jp.png",
                "zh-CN" => "pack://application:,,,/Resources/Images/Logos/logo-zh.png",
                "ko-KR" => "pack://application:,,,/Resources/Images/Logos/logo-ko.png",
                _       => "pack://application:,,,/Resources/Images/Logos/logo-en.png"
            };
            return new BitmapImage(new Uri(path));
        }
    }

    /// <summary>
    /// Switches the active language and notifies all XAML bindings to refresh.
    /// </summary>
    public void SetCulture(string cultureCode)
    {
        _culture = CultureInfo.GetCultureInfo(cultureCode);
        OnPropertyChanged("Item[]");
        OnPropertyChanged(nameof(LogoImage));
    }

    /// <summary>
    /// Returns a formatted string using the localized template for the given key.
    /// </summary>
    public string Format(string key, params object[] args)
    {
        var template = this[key];
        try { return string.Format(template, args); }
        catch { return template; }
    }
}
