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
using System.IO;
using System.Diagnostics;

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
                    await _chatController.JoinGroupAsync(c.Id); // Join ALL groups to listen for unread messages
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
                var fileType = IsVideoFile(openFileDialog.FileName) ? MessageType.Video : MessageType.File;
                var result = await _chatController.SendAttachmentMessageAsync(ViewModel.SelectedConversation.Id, App.CurrentUser.Id, openFileDialog.FileName, fileType);
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

        private void BtnDownloadAttachment_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.DataContext is Attachment attachment)
            {
                if (string.IsNullOrEmpty(attachment.FileUrl)) return;

                try
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        FileName = attachment.FileName,
                        Filter = "All files (*.*)|*.*"
                    };

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        // Because FileUrl currently stores the local path from the sender, 
                        // we can just copy it. In a real server scenario, this would be a Download/HttpClient call.
                        if (File.Exists(attachment.FileUrl))
                        {
                            File.Copy(attachment.FileUrl, saveFileDialog.FileName, true);
                            MessageBox.Show("Đã tải tệp xuống thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Không tìm thấy tệp nguồn để tải xuống.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi tải xuống: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnPlayMedia_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn)
            {
                var media = FindMediaElement(btn);
                if (media != null)
                {
                    media.MediaOpened -= ChatMediaElement_PreviewMediaOpened;
                    media.IsMuted = false;
                    media.Play();
                    SetVideoOverlayVisibility(btn, Visibility.Collapsed);
                    media.MediaEnded -= Media_MediaEnded;
                    media.MediaEnded += Media_MediaEnded;
                }
            }
        }

        private void BtnPauseMedia_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn)
            {
                var media = FindMediaElement(btn);
                if (media != null)
                {
                    media.Pause();
                    media.IsMuted = true;
                    SetVideoOverlayVisibility(btn, Visibility.Visible);
                }
            }
        }

        private void Media_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.MediaElement media)
            {
                media.Stop();
                media.Position = TimeSpan.Zero;
                media.IsMuted = true;
                media.Pause();
                if (VisualTreeHelper.GetParent(media) is System.Windows.Controls.Grid grid)
                {
                    SetVideoOverlayVisibility(grid, Visibility.Visible);
                }
            }
        }

        private void ChatMediaElement_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.MediaElement media) return;
            if (Equals(media.Tag, "PreviewInitialized")) return;

            media.Tag = "PreviewInitialized";
            media.MediaOpened -= ChatMediaElement_PreviewMediaOpened;
            media.MediaOpened += ChatMediaElement_PreviewMediaOpened;

            // Force open media once so we can capture/show the first frame as thumbnail.
            try
            {
                media.Play();
            }
            catch
            {
                // Ignore and keep fallback overlay.
            }
        }

        private void ChatMediaElement_PreviewMediaOpened(object? sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.MediaElement media) return;

            media.MediaOpened -= ChatMediaElement_PreviewMediaOpened;
            try
            {
                media.Play();
                media.Position = TimeSpan.FromMilliseconds(1);
                media.Pause();
            }
            catch
            {
                media.Position = TimeSpan.Zero;
                media.Pause();
            }
        }

        private System.Windows.Controls.MediaElement? FindMediaElement(DependencyObject child)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);
            while (parent != null && !(parent is System.Windows.Controls.Grid))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            if (parent is System.Windows.Controls.Grid grid)
            {
                return grid.Children.OfType<System.Windows.Controls.MediaElement>().FirstOrDefault();
            }
            return null;
        }

        private void SetVideoOverlayVisibility(DependencyObject child, Visibility visibility)
        {
            DependencyObject? parent = child;
            while (parent != null && parent is not System.Windows.Controls.Grid)
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            if (parent is System.Windows.Controls.Grid grid)
            {
                SetVideoOverlayVisibility(grid, visibility);
            }
        }

        private static void SetVideoOverlayVisibility(System.Windows.Controls.Grid grid, Visibility visibility)
        {
            foreach (var border in grid.Children.OfType<System.Windows.Controls.Border>())
            {
                if (border.Tag as string == "VideoOverlay")
                {
                    border.Visibility = visibility;
                }
            }

            foreach (var button in grid.Children.OfType<System.Windows.Controls.Button>())
            {
                if (button.Tag as string == "VideoPlayButton")
                {
                    button.Visibility = visibility;
                }
            }

        }

        private static bool IsVideoFile(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension is ".mp4" or ".mov" or ".avi" or ".mkv" or ".wmv" or ".webm";
        }
    }
}
