using System.Globalization;
using System.Reflection;
using System.Resources;
using CommunityToolkit.Mvvm.ComponentModel;
using KatyaKatya.Helpers;

namespace KatyaKatya.Localization;

public sealed class LocalizationManager : ObservableObject
{
    public static readonly LocalizationManager Instance = new();

    private static readonly ResourceManager ResourceManager = new(
        "KatyaKatya.Localization.Lang",
        Assembly.GetExecutingAssembly());

    private CultureInfo _culture = CultureInfo.GetCultureInfo("en-US");

    private LocalizationManager()
    {
    }

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

    public string? TryGet(string key)
    {
        try
        {
            return ResourceManager.GetString(key, _culture);
        }
        catch
        {
            return null;
        }
    }

    public string CurrentCultureCode => _culture.Name;

    public string LogoPath => LogoResolver.Resolve(_culture.Name, ThemeAssets.CurrentThemeName);

    public void SetCulture(string cultureCode)
    {
        _culture = CultureInfo.GetCultureInfo(cultureCode);
        CultureInfo.CurrentUICulture = _culture;
        OnPropertyChanged("Item[]");
        OnPropertyChanged(nameof(LogoPath));
    }

    public string Format(string key, params object[] args)
    {
        var template = this[key];
        try { return string.Format(template, args); }
        catch { return template; }
    }
}
