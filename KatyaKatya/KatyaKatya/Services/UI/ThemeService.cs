using System;
using System.Linq;
using Avalonia;
using Avalonia.Markup.Xaml.Styling;
using KatyaKatya.Services.Interfaces;

namespace KatyaKatya.Services.UI;

public class ThemeService : IThemeService
{
    private static readonly string PastelUri  = "avares://KatyaKatya/Resources/Themes/BaseTheme.axaml";
    private static readonly string SketchUri  = "avares://KatyaKatya/Resources/Themes/SketchTheme.axaml";
    private static readonly string BaseAvares = "avares://KatyaKatya/";

    public void ApplyTheme(string themeName)
    {
        var app = Application.Current;
        if (app is null) return;

        var merged = app.Resources.MergedDictionaries;

        // Remove the currently loaded theme dictionary (if any)
        var old = merged.OfType<ResourceInclude>()
            .FirstOrDefault(r => r.Source?.ToString().Contains("/Themes/") == true);
        if (old is not null)
            merged.Remove(old);

        var uri = themeName == "Sketch" ? SketchUri : PastelUri;
        merged.Add(new ResourceInclude(new Uri(BaseAvares)) { Source = new Uri(uri) });
    }
}
