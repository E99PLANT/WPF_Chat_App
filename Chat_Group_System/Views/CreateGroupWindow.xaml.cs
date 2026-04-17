using System.Windows;
using System.Windows.Input;

namespace Chat_Group_System.Views
{
    public partial class CreateGroupWindow : Window
    {
        public string GroupName { get; private set; } = string.Empty;

        public CreateGroupWindow()
        {
            InitializeComponent();
            txtGroupName.Focus();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtGroupName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên nhóm.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            GroupName = txtGroupName.Text.Trim();
            DialogResult = true;
            Close();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Enter)
            {
                BtnCreate_Click(this, new RoutedEventArgs());
            }
            else if (e.Key == Key.Escape)
            {
                BtnCancel_Click(this, new RoutedEventArgs());
            }
        }
    }
}