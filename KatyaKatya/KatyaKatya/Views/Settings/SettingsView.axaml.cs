using Avalonia.Controls;
using System.ComponentModel;
using KatyaKatya.ViewModels.Settings;
using KatyaKatya.Helpers;
using Avalonia.Media.Imaging;
using System;
using Avalonia.Platform;

namespace KatyaKatya.Views.Settings;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is SettingsViewModel vm)
        {
            vm.PropertyChanged += OnVmPropertyChanged;
            UpdateLogo(vm);
        }
    }

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SettingsViewModel.SelectedLanguage) ||
            e.PropertyName == nameof(SettingsViewModel.SelectedTheme))
        {
            if (DataContext is SettingsViewModel vm)
            {
                UpdateLogo(vm);
            }
        }
    }

    private void UpdateLogo(SettingsViewModel vm)
    {
        var lang = vm.SelectedLanguage?.Code ?? "en-US";
        var theme = vm.SelectedTheme ?? "Pastel";
        var logoUriString = LogoResolver.Resolve(lang, theme);
        
        try
        {
            var logoImage = this.FindControl<Image>("LogoImageControl");
            if (logoImage != null)
            {
                logoImage.Source = new Bitmap(AssetLoader.Open(new Uri(logoUriString)));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SettingsView] Error loading logo: {ex.Message}");
        }
    }
}
