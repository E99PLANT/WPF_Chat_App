using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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

            string currentPassword = btnToggleCurrentPassword.IsChecked == true ? txtVisibleCurrentPassword.Text : txtCurrentPassword.Password;
            string newPassword = btnToggleNewPassword.IsChecked == true ? txtVisibleNewPassword.Text : txtNewPassword.Password;
            string confirmNewPassword = btnToggleConfirmNewPassword.IsChecked == true ? txtVisibleConfirmNewPassword.Text : txtConfirmNewPassword.Password;

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

        private void BtnToggleCurrentPassword_Click(object sender, RoutedEventArgs e)
        {
            TogglePasswordVisibility(btnToggleCurrentPassword, txtCurrentPassword, txtVisibleCurrentPassword);
        }

        private void BtnToggleNewPassword_Click(object sender, RoutedEventArgs e)
        {
            TogglePasswordVisibility(btnToggleNewPassword, txtNewPassword, txtVisibleNewPassword);
        }

        private void BtnToggleConfirmNewPassword_Click(object sender, RoutedEventArgs e)
        {
            TogglePasswordVisibility(btnToggleConfirmNewPassword, txtConfirmNewPassword, txtVisibleConfirmNewPassword);
        }

        private void TogglePasswordVisibility(ToggleButton btn, PasswordBox pb, TextBox tb)
        {
            if (btn.IsChecked == true)
            {
                tb.Text = pb.Password;
                pb.Visibility = Visibility.Collapsed;
                tb.Visibility = Visibility.Visible;
                ((TextBlock)btn.Content).Text = "🙈";
            }
            else
            {
                pb.Password = tb.Text;
                tb.Visibility = Visibility.Collapsed;
                pb.Visibility = Visibility.Visible;
                ((TextBlock)btn.Content).Text = "👁";
            }
        }
    }
}