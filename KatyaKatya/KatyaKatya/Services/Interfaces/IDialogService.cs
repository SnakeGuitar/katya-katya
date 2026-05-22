namespace KatyaKatya.Services.Interfaces;

public enum DialogIcon
{
    Information,
    Warning,
    Error,
    Question
}

public enum DialogButton
{
    OK,
    OKCancel,
    YesNo
}

public enum DialogResult
{
    None,
    OK,
    Cancel,
    Yes,
    No
}

/// <summary>
/// Service for showing modal dialogs.
/// </summary>
public interface IDialogService
{
    DialogResult ShowMessage(string message, string title = "Katya Katya", DialogButton buttons = DialogButton.OK, DialogIcon icon = DialogIcon.Information);
}
