using System.Windows;
using System.Windows.Controls;
using MemoryGame.Client.ViewModels.Session;

namespace MemoryGame.Client.Views.Session;

public partial class RegisterView : UserControl
{
    public RegisterView()
    {
        InitializeComponent();

        PasswordBox.PasswordChanged += (_, _) =>
        {
            if (DataContext is RegisterViewModel vm)
                vm.Password = PasswordBox.Password;
        };

        ConfirmPasswordBox.PasswordChanged += (_, _) =>
        {
            if (DataContext is RegisterViewModel vm)
                vm.ConfirmPassword = ConfirmPasswordBox.Password;
        };
    }
}
