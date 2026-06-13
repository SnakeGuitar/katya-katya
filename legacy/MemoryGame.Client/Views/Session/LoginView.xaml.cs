using System.Windows;
using System.Windows.Controls;
using MemoryGame.Client.ViewModels.Session;

namespace MemoryGame.Client.Views.Session;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();

        PasswordBox.PasswordChanged += OnPasswordChanged;
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm)
            vm.Password = PasswordBox.Password;
    }
}
