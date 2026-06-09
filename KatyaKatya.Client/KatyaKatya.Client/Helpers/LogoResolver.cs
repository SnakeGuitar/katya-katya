namespace KatyaKatya.Helpers;

public static class LogoResolver
{
    public static string Resolve(string languageCode, string themeName)
    {
        string languagePrefix = languageCode.StartsWith("es", System.StringComparison.OrdinalIgnoreCase) ? "es" : "en";
        bool isSketch = string.Equals(themeName, "Sketch", System.StringComparison.OrdinalIgnoreCase);

        if (isSketch)
        {
            return $"avares://KatyaKatya/Resources/Images/Logos/sketch-logo-{languagePrefix}.png";
        }
        else
        {
            return $"avares://KatyaKatya/Resources/Images/Logos/logo-{languagePrefix}.png";
        }
    }
}
