using System.Windows;

namespace MemoryGame.Client;

/// <summary>
/// Single-window shell. All navigation happens via ContentControl + DataTemplates.
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
}
