using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    private SinglePlayerDifficulty _selectedDifficulty = SinglePlayerDifficulty.Easy;

    [ObservableProperty]
    private int _customCardCount = 16;

    [ObservableProperty]
    private int _customTurnTime = 60;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowCustomTurnTime))]
    private bool _customNoTimeLimit;

    public bool IsCustomMode => SelectedDifficulty == SinglePlayerDifficulty.Custom;
    public bool IsEasy       => SelectedDifficulty == SinglePlayerDifficulty.Easy;
    public bool IsMedium     => SelectedDifficulty == SinglePlayerDifficulty.Medium;
    public bool IsHard       => SelectedDifficulty == SinglePlayerDifficulty.Hard;
    public bool ShowCustomTurnTime => IsCustomMode && !CustomNoTimeLimit;

    public int[] CustomCardCountOptions { get; } = [4, 6, 8, 10, 12, 16, 20, 24, 28, 30, 36];

    public SinglePlayerMenuViewModel(INavigationService navigation)
    {
        _navigation = navigation;
    }

    [RelayCommand] private void SelectEasy()   => SelectedDifficulty = SinglePlayerDifficulty.Easy;
    [RelayCommand] private void SelectMedium() => SelectedDifficulty = SinglePlayerDifficulty.Medium;
    [RelayCommand] private void SelectHard()   => SelectedDifficulty = SinglePlayerDifficulty.Hard;
    [RelayCommand] private void SelectCustom() => SelectedDifficulty = SinglePlayerDifficulty.Custom;

    [RelayCommand]
    private void StartGame()
    {
        (int cards, int time) = SelectedDifficulty switch
        {
            SinglePlayerDifficulty.Easy   => (16, 60),
            SinglePlayerDifficulty.Medium => (24, 45),
            SinglePlayerDifficulty.Hard   => (36, 30),
            SinglePlayerDifficulty.Custom => (CustomCardCount, CustomNoTimeLimit ? 0 : CustomTurnTime),
            _                             => (16, 60),
        };

        _navigation.NavigateTo<SinglePlayerGameViewModel>(vm => vm.Initialize(cards, time));
    }

    [RelayCommand]
    private void GoBack()
    {
        if (_navigation.CanGoBack)
            _navigation.GoBack();
        else
            _navigation.NavigateToRootWithFade<MainMenuViewModel>();
    }
}
