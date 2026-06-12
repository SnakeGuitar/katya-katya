using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KatyaKatya.Localization;
using KatyaKatya.Services.Interfaces;
using KatyaKatya.ViewModels.MainMenu;

namespace KatyaKatya.ViewModels.SinglePlayer;

public enum SinglePlayerDifficulty { Easy, Medium, Hard, Custom }

public partial class SinglePlayerMenuViewModel : ObservableObject
{
    private readonly INavigationService _navigation;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCustomMode))]
    [NotifyPropertyChangedFor(nameof(IsEasy))]
    [NotifyPropertyChangedFor(nameof(IsMedium))]
    [NotifyPropertyChangedFor(nameof(IsHard))]
    [NotifyPropertyChangedFor(nameof(ShowCustomTotalTime))]
    private SinglePlayerDifficulty _selectedDifficulty = SinglePlayerDifficulty.Easy;

    [ObservableProperty]
    private int _customCardCount = 16;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CustomTotalTimeDisplay))]
    private int _customTotalTime = 180;

    [ObservableProperty]
    private double _customTotalTimeSliderValue = 180;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowCustomTotalTime))]
    private bool _customNoTimeLimit;

    public bool IsCustomMode => SelectedDifficulty == SinglePlayerDifficulty.Custom;
    public bool IsEasy       => SelectedDifficulty == SinglePlayerDifficulty.Easy;
    public bool IsMedium     => SelectedDifficulty == SinglePlayerDifficulty.Medium;
    public bool IsHard       => SelectedDifficulty == SinglePlayerDifficulty.Hard;
    public bool ShowCustomTotalTime => IsCustomMode && !CustomNoTimeLimit;
    public string CustomTotalTimeLabel => LocalizationManager.Instance.TryGet("SinglePlayer_Label_TotalTime") ?? "Total time";
    public string CustomTotalTimeDisplay => FormatTime(CustomTotalTime);

    public int[] CustomCardCountOptions { get; } = [4, 6, 8, 10, 12, 16, 20, 24, 28, 30, 36];

    public SinglePlayerMenuViewModel(INavigationService navigation)
    {
        _navigation = navigation;
    }

    partial void OnCustomTotalTimeSliderValueChanged(double value)
    {
        var snapped = Math.Clamp((int)Math.Round(value / 15.0) * 15, 30, 600);
        if (CustomTotalTime != snapped)
            CustomTotalTime = snapped;
    }

    partial void OnCustomTotalTimeChanged(int value)
    {
        var snapped = Math.Clamp((int)Math.Round(value / 15.0) * 15, 30, 600);
        if (value != snapped)
        {
            CustomTotalTime = snapped;
            return;
        }

        if (Math.Abs(CustomTotalTimeSliderValue - snapped) > double.Epsilon)
            CustomTotalTimeSliderValue = snapped;
    }

    [RelayCommand] private void SelectEasy()   => SelectedDifficulty = SinglePlayerDifficulty.Easy;
    [RelayCommand] private void SelectMedium() => SelectedDifficulty = SinglePlayerDifficulty.Medium;
    [RelayCommand] private void SelectHard()   => SelectedDifficulty = SinglePlayerDifficulty.Hard;
    [RelayCommand] private void SelectCustom() => SelectedDifficulty = SinglePlayerDifficulty.Custom;

    [RelayCommand]
    private void StartGame()
    {
        (int cards, int totalTime) = SelectedDifficulty switch
        {
            SinglePlayerDifficulty.Easy   => (16, 180),
            SinglePlayerDifficulty.Medium => (24, 240),
            SinglePlayerDifficulty.Hard   => (36, 300),
            SinglePlayerDifficulty.Custom => (CustomCardCount, CustomNoTimeLimit ? 0 : CustomTotalTime),
            _                             => (16, 180),
        };

        _navigation.NavigateTo<SinglePlayerGameViewModel>(vm => vm.Initialize(cards, totalTime));
    }

    [RelayCommand]
    private void GoBack()
    {
        if (_navigation.CanGoBack)
            _navigation.GoBack();
        else
            _navigation.NavigateToRootWithFade<MainMenuViewModel>();
    }

    private static string FormatTime(int seconds)
        => TimeSpan.FromSeconds(seconds).ToString(@"mm\:ss");
}
