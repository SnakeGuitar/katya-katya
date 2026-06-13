using MemoryGame.Client.ViewModels.Common;
using MemoryGame.Client.Views.Common;
using System.Windows;

namespace MemoryGame.Client.Services.UI;
using MemoryGame.Client.Services.Interfaces;

/// <summary>
/// Practical implementation of IDialogService that shows a custom WPF Window.
/// </summary>
public class DialogService : IDialogService
{
    public DialogResult ShowMessage(string message, string title = "Memory Game", DialogButton buttons = DialogButton.OK, DialogIcon icon = DialogIcon.Information)
    {
        var owner = Application.Current.MainWindow;
        
        var dialog = new DialogWindow();
        var viewModel = new DialogViewModel(dialog)
        {
            Title = title,
            Message = message,
            Buttons = buttons,
            Icon = icon
        };
        
        dialog.DataContext = viewModel;
        dialog.Owner = owner;
        
        dialog.ShowDialog();
        
        return viewModel.Result;
    }
}
