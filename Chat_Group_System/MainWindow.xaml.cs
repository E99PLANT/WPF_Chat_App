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
        public MainViewModel ViewModel { get; }

        public MainWindow(ChatController chatController)
        {
            InitializeComponent();
            _chatController = chatController;
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
                    ViewModel.Conversations.Add(new ConversationViewModel(c));
                }

                // 2. Mặc định chọn nhóm đầu tiên nếu có
                if (ViewModel.Conversations.Count > 0)
                {
                    ViewModel.SelectedConversation = ViewModel.Conversations[0];
                    ConvList.SelectedItem = ViewModel.SelectedConversation;
                    await LoadMessagesForConversation(ViewModel.SelectedConversation.Id);
                }
            }
        }

        private async System.Threading.Tasks.Task LoadMessagesForConversation(int conversationId)
        {
            ViewModel.CurrentMessages.Clear();
            var messages = await _chatController.GetRecentMessagesAsync(conversationId);
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
                    var vm = new ConversationViewModel(result.Group);
                    ViewModel.Conversations.Add(vm);
                    ViewModel.SelectedConversation = vm;
                    ConvList.SelectedItem = vm;
                    await LoadMessagesForConversation(vm.Id);
                }
            }
        }

        private void BtnGroupSettings_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedConversation == null)
            {
                MessageBox.Show("Please select a conversation first.");
                return;
            }

            var groupSettingsWin = App.ServiceProvider.GetRequiredService<Views.GroupSettingsWindow>();
            groupSettingsWin.Owner = this;
            groupSettingsWin.ShowDialog();
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
