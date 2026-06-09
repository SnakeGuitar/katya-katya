namespace KatyaKatya.Helpers;

public static class LogoResolver
{
    public static string Resolve(string languageCode, string themeName)
    {
        string languagePrefix = languageCode switch
        {
            not null when languageCode.StartsWith("es", System.StringComparison.OrdinalIgnoreCase) => "es",
            not null when languageCode.StartsWith("ja", System.StringComparison.OrdinalIgnoreCase) => "jp",
            not null when languageCode.StartsWith("zh", System.StringComparison.OrdinalIgnoreCase) => "zh",
            not null when languageCode.StartsWith("ko", System.StringComparison.OrdinalIgnoreCase) => "ko",
            _ => "en"
        };

        return ThemeAssets.GetLogoPath(themeName, languagePrefix);
    }
}
