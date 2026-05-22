using Avalonia.Controls;
using Avalonia.Interactivity;
using KatyaKatya.ViewModels;
using KatyaKatya.Services.Interfaces;
using System;

namespace KatyaKatya.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            if (vm.Navigation is System.ComponentModel.INotifyPropertyChanged npc)
            {
                npc.PropertyChanged += OnNavigationPropertyChanged;
            }
            UpdateBackground(vm.Navigation.CurrentViewModel);
        }
    }

    private void OnNavigationPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(INavigationService.CurrentViewModel))
        {
            if (DataContext is MainWindowViewModel vm)
            {
                UpdateBackground(vm.Navigation.CurrentViewModel);
            }
        }
    }

    private void UpdateBackground(object? nextViewModel)
    {
        if (nextViewModel == null) return;

        var bgImage = this.FindControl<Image>("GlobalBgImage");
        if (bgImage == null) return;

        var name = nextViewModel.GetType().Name;
        string uriStr = name is "MainMenuViewModel" or "MoreMenuViewModel"
                             or "LobbyMenuViewModel" or "HostLobbyViewModel" or "LobbyViewModel"
            ? "avares://KatyaKatya/Resources/Images/Backgrounds/katya-main-background-only.png"
            : "avares://KatyaKatya/Resources/Images/Backgrounds/background-minimalistic.png";

        try
        {
            var assets = Avalonia.Platform.AssetLoader.Open(new Uri(uriStr));
            bgImage.Source = new Avalonia.Media.Imaging.Bitmap(assets);
        }
        catch { }
    }

    private void OnCloseClicked(object? sender, RoutedEventArgs e) => Close();

    private void OnMinimizeClicked(object? sender, RoutedEventArgs e)
        => WindowState = WindowState.Minimized;

    private void OnMaximizeClicked(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;

        if (MainWindowBorder != null)
        {
            MainWindowBorder.CornerRadius = WindowState == WindowState.Maximized
                ? new Avalonia.CornerRadius(0)
                : new Avalonia.CornerRadius(14);
        }
    }

    private void OnMenuDropdownItemClicked(object? sender, RoutedEventArgs e)
    {
        if (MenuToggleBtn != null)
            MenuToggleBtn.IsChecked = false;
    }
}
