using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Chat_Group_System.Controllers;

namespace Chat_Group_System.Views
{
    public partial class ChangePasswordWindow : Window
    {
        private readonly UserController _userController;

        public ChangePasswordWindow()
        {
            InitializeComponent();
            _userController = App.ServiceProvider.GetRequiredService<UserController>();
        }

        private async void BtnUpdatePassword_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser == null)
            {
                MessageBox.Show("You must be logged in to change your password.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string currentPassword = txtCurrentPassword.Password;
            string newPassword = txtNewPassword.Password;
            string confirmNewPassword = txtConfirmNewPassword.Password;

            var result = await _userController.ChangePasswordAsync(App.CurrentUser.Id, currentPassword, newPassword, confirmNewPassword);

            if (result.Success)
            {
                MessageBox.Show(result.Message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                // Optionally log them out or close window
                var loginWindow = App.ServiceProvider.GetRequiredService<LoginWindow>();
                loginWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show(result.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBackToLogin_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var loginWindow = App.ServiceProvider.GetRequiredService<LoginWindow>();
            loginWindow.Show();
            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = App.ServiceProvider.GetRequiredService<LoginWindow>();
            loginWindow.Show();
            this.Close();
        }
    }
}