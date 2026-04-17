using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Chat_Group_System.Controllers;

namespace Chat_Group_System.Views
{
    public partial class LoginWindow : Window
    {
        private readonly UserController _userController;

        public LoginWindow()
        {
            InitializeComponent();
            _userController = App.ServiceProvider.GetRequiredService<UserController>();
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text;
            string password = btnTogglePassword.IsChecked == true ? txtVisiblePassword.Text : txtPassword.Password;

            var result = await _userController.LoginAsync(email, password);

            if (result.Success)
            {
                App.CurrentUser = result.SessionUser;
                // Navigate to MainWindow
                var mainWindow = App.ServiceProvider.GetRequiredService<MainWindow>();
                mainWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show(result.Message, "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRegister_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var registerWindow = App.ServiceProvider.GetRequiredService<RegisterWindow>();
            registerWindow.Show();
            this.Close();
        }

        private void BtnTogglePassword_Click(object sender, RoutedEventArgs e)
        {
            if (btnTogglePassword.IsChecked == true)
            {
                // Show Password
                txtVisiblePassword.Text = txtPassword.Password;
                txtPassword.Visibility = Visibility.Collapsed;
                txtVisiblePassword.Visibility = Visibility.Visible;
                ((TextBlock)((System.Windows.Controls.Primitives.ToggleButton)sender).Content).Text = "🙈";
            }
            else
            {
                // Hide Password
                txtPassword.Password = txtVisiblePassword.Text;
                txtVisiblePassword.Visibility = Visibility.Collapsed;
                txtPassword.Visibility = Visibility.Visible;
                ((TextBlock)((System.Windows.Controls.Primitives.ToggleButton)sender).Content).Text = "👁";
            }
        }

        private void BtnForgotPassword_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var changePasswordWindow = App.ServiceProvider.GetRequiredService<ChangePasswordWindow>();
            changePasswordWindow.Show();
            this.Close();
        }
    }
}