using System.Windows;

namespace Chat_Group_System.Views
{
    public partial class CreateDMWindow : Window
    {
        public string SearchTerm { get; private set; } = string.Empty;

        public CreateDMWindow()
        {
            InitializeComponent();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            var searchStr = txtSearchTerm.Text.Trim();
            if (string.IsNullOrWhiteSpace(searchStr))
            {
                MessageBox.Show("Vui lòng nhập Email hoặc Tên người muốn chat.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SearchTerm = searchStr;
            DialogResult = true;
            Close();
        }
    }
}