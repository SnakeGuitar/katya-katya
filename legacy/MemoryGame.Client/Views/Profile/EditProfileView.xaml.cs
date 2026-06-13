using System.Windows;
using System.Windows.Controls;
using MemoryGame.Client.ViewModels.Profile;

namespace MemoryGame.Client.Views.Profile;

public partial class EditProfileView : UserControl
{
    public EditProfileView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// PasswordBox doesn't support binding — push values to the ViewModel manually.
    /// </summary>
    private void CurrentPassword_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is EditProfileViewModel vm)
            vm.CurrentPassword = ((PasswordBox)sender).Password;
    }

    private void NewPassword_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is EditProfileViewModel vm)
            vm.NewPassword = ((PasswordBox)sender).Password;
    }
}
