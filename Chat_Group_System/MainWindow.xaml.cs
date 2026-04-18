using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Data;
using Chat_Group_System.Controllers;
using Chat_Group_System.Models.Entities;
using Chat_Group_System.ViewModels;
using Microsoft.Win32;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Linq;

namespace Chat_Group_System
{
    public partial class MainWindow : Window
    {
        private readonly ChatController _chatController;
        private readonly UserController _userController;
        public MainViewModel ViewModel { get; }

        public MainWindow(ChatController chatController, UserController userController)
        {
            InitializeComponent();
            _chatController = chatController;
            _userController = userController;
            ViewModel = new MainViewModel(_chatController);
            DataContext = ViewModel;

            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser != null)
            {
                await _chatController.ConnectRealtimeAsync(App.CurrentUser.Id);

                // 1. Tải danh sách nhóm chat từ DB
                var convos = await _chatController.GetUserConversationsAsync(App.CurrentUser.Id);
                foreach (var c in convos)
                {
                    ViewModel.Conversations.Add(new ConversationViewModel(c, App.CurrentUser.Id));
                }

                // 2. Mặc định chọn nhóm đầu tiên nếu có
                if (ViewModel.Conversations.Count > 0)
                {
                    ViewModel.SelectedConversation = ViewModel.Conversations[0];
                    // Gán SelectedItem sẽ tự động gọi ConvList_SelectionChanged và tải tin nhắn
                    ConvList.SelectedItem = ViewModel.SelectedConversation;
                }
            }
        }

        private async System.Threading.Tasks.Task LoadMessagesForConversation(int conversationId)
        {
            ViewModel.CurrentMessages.Clear();

            // Refresh Member List and determine if current user is still in the group
            // Use scoped controller to prevent concurrency conflicts with incoming SignalR updates
            using var scope = App.ServiceProvider.CreateScope();
            var scopedController = scope.ServiceProvider.GetRequiredService<ChatController>();

            var members = await scopedController.GetGroupMembersAsync(conversationId);
            ViewModel.CanSendMessage = members.Any(m => m.UserId == App.CurrentUser?.Id);

            var messages = await scopedController.GetRecentMessagesAsync(conversationId);
            foreach (var m in messages)
            {
                ViewModel.CurrentMessages.Add(new MessageViewModel(m));
            }
            ScrollToBottom();
        }

        private void ScrollToBottom()
        {
            if (MessageScrollViewer != null)
            {
                MessageScrollViewer.Dispatcher.InvokeAsync(() =>
                {
                    MessageScrollViewer.ScrollToEnd();
                }, System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        private async void MainWindow_Closed(object? sender, EventArgs e)
        {
            ViewModel.Cleanup();
            await _chatController.DisconnectRealtimeAsync();
        }

        private async void ConvList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ConvList.SelectedItem is ConversationViewModel selected)
            {
                ViewModel.SelectedConversation = selected;
                await LoadMessagesForConversation(selected.Id);
            }
        }

        private async void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            await SendMessageAction();
        }

        private void TxtInputMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers != ModifierKeys.Shift)
            {
                e.Handled = true;
                _ = SendMessageAction();
            }
        }

        private async System.Threading.Tasks.Task SendMessageAction()
        {
            if (App.CurrentUser == null || ViewModel.SelectedConversation == null) return;

            string txtContent = ViewModel.InputText;
            if (string.IsNullOrWhiteSpace(txtContent) || txtContent == "Nhập tin nhắn...") return;

            var result = await _chatController.SendTextMessageAsync(ViewModel.SelectedConversation.Id, App.CurrentUser.Id, txtContent);
            if (result.Success && result.SentMessage != null)
            {
                ViewModel.CurrentMessages.Add(new MessageViewModel(result.SentMessage));
                ViewModel.InputText = "";
                ScrollToBottom();
            }
            else
            {
                MessageBox.Show(result.Message, "Error Sending Message", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnSelectFile_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser == null || ViewModel.SelectedConversation == null)
            {
                MessageBox.Show("Vui lòng chọn một cuộc trò chuyện.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog() { Title = "Select a file", Filter = "All files (*.*)|*.*" };
            if (openFileDialog.ShowDialog() == true)
            {
                var result = await _chatController.SendAttachmentMessageAsync(ViewModel.SelectedConversation.Id, App.CurrentUser.Id, openFileDialog.FileName, MessageType.File);
                if (result.Success && result.SentMessage != null)
                {
                    ViewModel.CurrentMessages.Add(new MessageViewModel(result.SentMessage));
                    ScrollToBottom();
                }
            }
        }

        private async void BtnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser == null || ViewModel.SelectedConversation == null) return;

            OpenFileDialog openFileDialog = new OpenFileDialog() { Title = "Select an image", Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg" };
            if (openFileDialog.ShowDialog() == true)
            {
                var result = await _chatController.SendAttachmentMessageAsync(ViewModel.SelectedConversation.Id, App.CurrentUser.Id, openFileDialog.FileName, MessageType.Image);
                if (result.Success && result.SentMessage != null)
                {
                    ViewModel.CurrentMessages.Add(new MessageViewModel(result.SentMessage));
                    ScrollToBottom();
                }
            }
        }

        private void BtnEmoji_Click(object sender, RoutedEventArgs e)
        {
            EmojiPickerPopup.IsOpen = true;
        }

        private void BtnEmojiPick_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Content is string emoji)
            {
                if (ViewModel.InputText == "Nhập tin nhắn..." || string.IsNullOrWhiteSpace(ViewModel.InputText))
                {
                    ViewModel.InputText = emoji;
                }
                else
                {
                    ViewModel.InputText += emoji;
                }
                txtInputMessage.Focus();
                txtInputMessage.CaretIndex = ViewModel.InputText.Length;
            }
            EmojiPickerPopup.IsOpen = false;
        }

        private async void BtnNewChat_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser == null) return;

            var createGroupWin = App.ServiceProvider.GetRequiredService<Views.CreateGroupWindow>();
            createGroupWin.Owner = this;
            if (createGroupWin.ShowDialog() == true)
            {
                string groupName = createGroupWin.GroupName;
                var result = await _chatController.CreateGroupAsync(App.CurrentUser.Id, groupName, new System.Collections.Generic.List<int>());
                if (result.Success && result.Group != null)
                {
                    var vm = new ConversationViewModel(result.Group, App.CurrentUser.Id);
                    ViewModel.Conversations.Add(vm);
                    ViewModel.SelectedConversation = vm;
                    ConvList.SelectedItem = vm;
                }
            }
        }

        private async void BtnNewDM_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser == null) return;

            var createDMWin = new Views.CreateDMWindow
            {
                Owner = this
            };

            if (createDMWin.ShowDialog() == true)
            {
                string targetSearchTerm = createDMWin.SearchTerm;
                var targetUser = await _userController.GetUserByEmailOrNameAsync(targetSearchTerm);
                
                if (targetUser == null)
                {
                    MessageBox.Show("User not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                if (targetUser.Id == App.CurrentUser.Id)
                {
                    MessageBox.Show("You cannot direct message yourself.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var dmResult = await _chatController.CreateOrGetDirectMessageAsync(App.CurrentUser.Id, targetUser.Id);

                if (dmResult.Success && dmResult.Conversation != null)
                {
                    var existingVm = ViewModel.Conversations.FirstOrDefault(c => c.Id == dmResult.Conversation.Id);
                    if (existingVm != null)
                    {
                        ViewModel.SelectedConversation = existingVm;
                        ConvList.SelectedItem = existingVm;
                    }
                    else
                    {
                        var vm = new ConversationViewModel(dmResult.Conversation, App.CurrentUser.Id);
                        ViewModel.Conversations.Add(vm);
                        ViewModel.SelectedConversation = vm;
                        ConvList.SelectedItem = vm;
                    }
                }
                else
                {
                    MessageBox.Show(dmResult.Message ?? "Failed to create DM", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                // Disconnect SignalR real-time stream
                await _chatController.DisconnectRealtimeAsync();
                
                // Clear current user
                App.CurrentUser = null;
                ViewModel.CurrentMessages.Clear();
                ViewModel.Conversations.Clear();

                // Open Login Window
                var userController = App.ServiceProvider.GetRequiredService<UserController>();
                var loginWin = new Views.LoginWindow(userController);
                loginWin.Show();

                // Close current window
                this.Close();
            }
        }

        private async void BtnGroupSettings_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedConversation == null)
            {
                MessageBox.Show("Please select a conversation first.");
                return;
            }

            var groupSettingsWin = App.ServiceProvider.GetRequiredService<Views.GroupSettingsWindow>();
            var conversationResult = await _chatController.GetUserConversationsAsync(App.CurrentUser.Id);
            var conversationModel = conversationResult.FirstOrDefault(c => c.Id == ViewModel.SelectedConversation.Id);
            
            if (conversationModel != null)
            {
                groupSettingsWin.SetConversation(conversationModel);
                groupSettingsWin.Owner = this;
                groupSettingsWin.ShowDialog();
            }
            else
            {
                MessageBox.Show("Không tìm thấy thông tin nhóm.");
            }
        }

        private void UserProfile_Click(object sender, MouseButtonEventArgs e)
        {
            // Placeholder
        }

        private void TxtInputMessage_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ViewModel.InputText == "Nhập tin nhắn...")
                ViewModel.InputText = "";
        }

        private void TxtInputMessage_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ViewModel.InputText))
                ViewModel.InputText = "Nhập tin nhắn...";
        }

        private void TxtSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SearchText == "Search...")
                ViewModel.SearchText = "";
        }

        private void TxtSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ViewModel.SearchText))
                ViewModel.SearchText = "Search...";
        }

        private void TxtSearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ViewModel.SearchText = ((System.Windows.Controls.TextBox)sender).Text;
        }
    }
}
