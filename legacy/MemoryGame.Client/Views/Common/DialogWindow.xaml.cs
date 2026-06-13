using System.Windows;
using System.Windows.Input;

namespace MemoryGame.Client.Views.Common;

/// <summary>
/// Interaction logic for DialogWindow.xaml
/// </summary>
public partial class DialogWindow : Window
{
    public DialogWindow()
    {
        InitializeComponent();
        
        this.MouseDown += (s, e) =>
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        };
    }
}
