using System.Globalization;
using System.Reflection;
using System.Resources;
using CommunityToolkit.Mvvm.ComponentModel;
using KatyaKatya.Helpers;

namespace KatyaKatya.Localization;

public sealed class LocalizationManager : ObservableObject
{
    public static readonly LocalizationManager Instance = new();

    private static readonly IReadOnlyDictionary<string, string> KeyAliases = new Dictionary<string, string>
    {
        ["Session_Field_Username"] = "Global_Label_Username",
        ["Session_Field_Password"] = "Global_Label_Password",
        ["Session_Field_Email"] = "Global_Label_Email",
        ["Session_Field_ConfirmPassword"] = "Global_Label_ConfirmPassword",
        ["Session_Guest_Title"] = "GuestLogin_Title",
        ["Session_Guest_Description"] = "GuestLogin_Description",
        ["Session_Setup_Title"] = "SetupProfile_Title",
        ["Session_Setup_Description"] = "SetupProfile_Description",
        ["Session_Setup_SelectPhoto"] = "SetupProfile_SelectAvatar",
        ["Global_Button_Continue"] = "Global_Button_Guest",
        ["Settings_Title"] = "Settings_Label_Language",
        ["Settings_Language"] = "Settings_Label_Language",
        ["Settings_Music"] = "Settings_Label_Music",
        ["Settings_Fullscreen"] = "Settings_Label_Fullscreen",
        ["Settings_Back"] = "Global_Button_Back"
    };

    private static readonly ResourceManager ResourceManager = new(
        "KatyaKatya.Localization.Lang",
        Assembly.GetExecutingAssembly());

    private CultureInfo _culture = CultureInfo.GetCultureInfo("en-US");

    private LocalizationManager()
    {
        ThemeAssets.ThemeChanged += () =>
        {
            OnPropertyChanged(nameof(LogoPath));
            OnPropertyChanged(nameof(GlobalBackgroundPath));
            OnPropertyChanged(nameof(MainMenuBackgroundPath));
            OnPropertyChanged(nameof(MenuIconPath));
        };
    }

    public string this[string key]
    {
        get
        {
            try
            {
                return ResourceManager.GetString(ResolveKey(key), _culture) ?? $"[{key}]";
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
            return ResourceManager.GetString(ResolveKey(key), _culture);
        }
        catch
        {
            return null;
        }
    }

    public string CurrentCultureCode => _culture.Name;

    public string LogoPath => LogoResolver.Resolve(_culture.Name, ThemeAssets.CurrentThemeName);
    public string GlobalBackgroundPath => ThemeAssets.GetGlobalBackgroundPath(ThemeAssets.CurrentThemeName);
    public string MainMenuBackgroundPath => ThemeAssets.GetMainMenuBackgroundPath(ThemeAssets.CurrentThemeName);
    public string MenuIconPath => ThemeAssets.GetMenuIconPath(ThemeAssets.CurrentThemeName);

    public void SetCulture(string cultureCode)
    {
        _culture = CultureInfo.GetCultureInfo(cultureCode);
        CultureInfo.CurrentUICulture = _culture;
        OnPropertyChanged("Item[]");
        OnPropertyChanged(nameof(LogoPath));
        OnPropertyChanged(nameof(GlobalBackgroundPath));
        OnPropertyChanged(nameof(MainMenuBackgroundPath));
    }

    public string Format(string key, params object[] args)
    {
        var template = this[key];
        try { return string.Format(template, args); }
        catch { return template; }
    }

    private static string ResolveKey(string key) =>
        KeyAliases.TryGetValue(key, out var alias) ? alias : key;
}
