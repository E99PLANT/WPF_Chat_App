using System.Windows;
using Microsoft.Extensions.DependencyInjection;
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
            string password = txtPassword.Password;
            string confirmPassword = txtConfirmPassword.Password;

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

        private void BtnLogin_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var loginWindow = App.ServiceProvider.GetRequiredService<LoginWindow>();
            loginWindow.Show();
            this.Close();
        }
    }
}