using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KatyaKatya.Services.Interfaces;
using System;

namespace KatyaKatya.ViewModels.Common;

/// <summary>
/// Simple ViewModel for the custom DialogWindow.
/// </summary>
public partial class DialogViewModel : ObservableObject
{
    private readonly Action _closeAction;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsOkVisible))]
    [NotifyPropertyChangedFor(nameof(IsCancelVisible))]
    [NotifyPropertyChangedFor(nameof(IsYesNoVisible))]
    private string _title = "Katya Katya";

    [ObservableProperty]
    private string _message = string.Empty;

    [ObservableProperty]
    private DialogIcon _icon = DialogIcon.Information;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsOkVisible))]
    [NotifyPropertyChangedFor(nameof(IsCancelVisible))]
    [NotifyPropertyChangedFor(nameof(IsYesNoVisible))]
    private DialogButton _buttons = DialogButton.OK;

    public DialogResult Result { get; private set; } = DialogResult.None;

    public bool IsOkVisible => 
        Buttons == DialogButton.OK || Buttons == DialogButton.OKCancel;

    public bool IsCancelVisible => 
        Buttons == DialogButton.OKCancel;

    public bool IsYesNoVisible => 
        Buttons == DialogButton.YesNo;

    public DialogViewModel(Action closeAction)
    {
        _closeAction = closeAction;
    }

    [RelayCommand]
    private void OK()
    {
        Result = DialogResult.OK;
        _closeAction();
    }

    [RelayCommand]
    private void Cancel()
    {
        Result = DialogResult.Cancel;
        _closeAction();
    }

    [RelayCommand]
    private void Yes()
    {
        Result = DialogResult.Yes;
        _closeAction();
    }

    [RelayCommand]
    private void No()
    {
        Result = DialogResult.No;
        _closeAction();
    }
}
