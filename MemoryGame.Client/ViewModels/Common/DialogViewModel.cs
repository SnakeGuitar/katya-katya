using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemoryGame.Client.Services.Core;
using MemoryGame.Client.Services.Interfaces;
using MemoryGame.Client.Services.Media;
using MemoryGame.Client.Services.Network;
using MemoryGame.Client.Services.UI;
using System.Windows;

namespace MemoryGame.Client.ViewModels.Common;

/// <summary>
/// Simple ViewModel for the DialogWindow.
/// </summary>
public partial class DialogViewModel : ObservableObject
{
    private readonly Window _owner;

    [ObservableProperty] private string _title = "Memory Game";
    [ObservableProperty] private string _message = string.Empty;
    [ObservableProperty] private DialogIcon _icon = DialogIcon.Information;
    [ObservableProperty] private DialogButton _buttons = DialogButton.OK;

    public DialogResult Result { get; private set; } = DialogResult.None;

    public Visibility OKVisibility => 
        (Buttons == DialogButton.OK || Buttons == DialogButton.OKCancel) ? Visibility.Visible : Visibility.Collapsed;

    public Visibility CancelVisibility => 
        (Buttons == DialogButton.OKCancel) ? Visibility.Visible : Visibility.Collapsed;

    public Visibility YesNoVisibility => 
        (Buttons == DialogButton.YesNo) ? Visibility.Visible : Visibility.Collapsed;

    public DialogViewModel(Window owner)
    {
        _owner = owner;
    }

    [RelayCommand]
    private void OK()
    {
        Result = DialogResult.OK;
        _owner.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Result = DialogResult.Cancel;
        _owner.Close();
    }

    [RelayCommand]
    private void Yes()
    {
        Result = DialogResult.Yes;
        _owner.Close();
    }

    [RelayCommand]
    private void No()
    {
        Result = DialogResult.No;
        _owner.Close();
    }
}
