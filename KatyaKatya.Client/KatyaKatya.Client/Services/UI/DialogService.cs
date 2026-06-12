using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using KatyaKatya.Services.Interfaces;
using KatyaKatya.ViewModels.Common;
using KatyaKatya.Views.Common;

namespace KatyaKatya.Services.UI;

/// <summary>
/// Practical implementation of IDialogService that shows a custom Avalonia Window.
/// </summary>
public class DialogService : IDialogService
{
    public DialogResult ShowMessage(string message, string title = "Katya Katya",
        DialogButton buttons = DialogButton.OK, DialogIcon icon = DialogIcon.Information)
    {
        var owner = GetMainWindow();

        var dialog = new DialogWindow();
        var viewModel = new DialogViewModel(() => dialog.Close())
        {
            Title = title,
            Message = message,
            Buttons = buttons,
            Icon = icon
        };

        dialog.DataContext = viewModel;

        // Push nested dispatcher frame to block synchronously while keeping the UI responsive
        var frame = new DispatcherFrame();
        dialog.Closed += (s, e) => frame.Continue = false;

        if (owner != null)
        {
            // ShowDialog (vs Show) disables the owner window — true modality
            _ = dialog.ShowDialog(owner);
        }
        else
        {
            dialog.Show();
        }

        Dispatcher.UIThread.PushFrame(frame);

        return viewModel.Result;
    }

    private static Window? GetMainWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return desktop.MainWindow;
        return null;
    }
}
