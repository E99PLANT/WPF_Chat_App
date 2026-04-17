using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Chat_Group_System.Controllers;

namespace Chat_Group_System.Views
{
    public partial class RegisterWindow : Window
    {
        private readonly UserController _userController;

        public RegisterWindow()
        {
            InitializeComponent();
            _userController = App.ServiceProvider.GetRequiredService<UserController>();
        }

        private async void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            string fullName = txtFullName.Text;
            string email = txtEmail.Text;
            string password = btnTogglePassword.IsChecked == true ? txtVisiblePassword.Text : txtPassword.Password;
            string confirmPassword = btnToggleConfirmPassword.IsChecked == true ? txtVisibleConfirmPassword.Text : txtConfirmPassword.Password;

            var result = await _userController.RegisterAsync(fullName, email, password, confirmPassword);

            if (result.Success)
            {
                MessageBox.Show(result.Message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                var loginWindow = App.ServiceProvider.GetRequiredService<LoginWindow>();
                loginWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show(result.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnTogglePassword_Click(object sender, RoutedEventArgs e)
        {
            if (btnTogglePassword.IsChecked == true)
            {
                txtVisiblePassword.Text = txtPassword.Password;
                txtPassword.Visibility = Visibility.Collapsed;
                txtVisiblePassword.Visibility = Visibility.Visible;
                ((TextBlock)((System.Windows.Controls.Primitives.ToggleButton)sender).Content).Text = "🙈";
            }
            else
            {
                txtPassword.Password = txtVisiblePassword.Text;
                txtVisiblePassword.Visibility = Visibility.Collapsed;
                txtPassword.Visibility = Visibility.Visible;
                ((TextBlock)((System.Windows.Controls.Primitives.ToggleButton)sender).Content).Text = "👁";
            }
        }

        private void BtnToggleConfirmPassword_Click(object sender, RoutedEventArgs e)
        {
            if (btnToggleConfirmPassword.IsChecked == true)
            {
                txtVisibleConfirmPassword.Text = txtConfirmPassword.Password;
                txtConfirmPassword.Visibility = Visibility.Collapsed;
                txtVisibleConfirmPassword.Visibility = Visibility.Visible;
                ((TextBlock)((System.Windows.Controls.Primitives.ToggleButton)sender).Content).Text = "🙈";
            }
            else
            {
                txtConfirmPassword.Password = txtVisibleConfirmPassword.Text;
                txtVisibleConfirmPassword.Visibility = Visibility.Collapsed;
                txtConfirmPassword.Visibility = Visibility.Visible;
                ((TextBlock)((System.Windows.Controls.Primitives.ToggleButton)sender).Content).Text = "👁";
            }
        }

        private void BtnLogin_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var loginWindow = App.ServiceProvider.GetRequiredService<LoginWindow>();
            loginWindow.Show();
            this.Close();
        }
    }
}