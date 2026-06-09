namespace KatyaKatya.Services.Interfaces;

public interface IThemeService
{
    string CurrentThemeName { get; }

    void ApplyTheme(string themeName);
}
