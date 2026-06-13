using System.Windows;

namespace MemoryGame.Client.Services.Interfaces;

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
/// Service for showing custom modal dialogs.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Shows a message box with a custom "Pastel" theme.
    /// </summary>
    DialogResult ShowMessage(string message, string title = "Memory Game", DialogButton buttons = DialogButton.OK, DialogIcon icon = DialogIcon.Information);
}
