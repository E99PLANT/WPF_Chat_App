using System.Windows;
using Microsoft.Extensions.DependencyInjection;
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
            string password = txtPassword.Password;

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

        private void BtnForgotPassword_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var changePasswordWindow = App.ServiceProvider.GetRequiredService<ChangePasswordWindow>();
            changePasswordWindow.Show();
            this.Close();
        }
    }
}