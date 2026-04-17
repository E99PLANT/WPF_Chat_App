using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Data;
using Chat_Group_System.Controllers;
using Chat_Group_System.Models.Entities;
using Microsoft.Win32;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Specialized;

namespace Chat_Group_System
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly ChatController _chatController;
        
        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        // ── DataBinding Collections (MVVM Prep) ────────────────
        public ObservableCollection<Conversation> Conversations { get; set; } = new();
        public ICollectionView ConversationsView { get; private set; }
        public ObservableCollection<Message> CurrentMessages { get; set; } = new();
        
        private Conversation? _selectedConversation;
        public Conversation? SelectedConversation 
        { 
            get => _selectedConversation;
            set 
            {
                if (_selectedConversation != value)
                {
                    _selectedConversation = value;
                    OnPropertyChanged(); // Thông báo để Header cập nhật lại Tên và DataContext
                }
            }
        }

        public MainWindow(ChatController chatController)
        {
            InitializeComponent();
            _chatController = chatController;
            
            ConversationsView = CollectionViewSource.GetDefaultView(Conversations);
            ConversationsView.Filter = FilterConversation;

            // Set DataContext for UI bindings
            this.DataContext = this;

            this.Loaded += MainWindow_Loaded;
            this.Closed += MainWindow_Closed;

            // Đăng ký sự kiện: Cứ mỗi lần thay đổi số lượng tin nhắn (Add mới) -> Tự cuộn chuột xuống cùng
            CurrentMessages.CollectionChanged += CurrentMessages_CollectionChanged;
        }

        private void CurrentMessages_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Reset)
            {
                // Dùng Dispatcher.InvokeAsync để đảm bảo UI kịp render bong bóng mới trước khi mình cuộn xuống
                MessageScrollViewer.Dispatcher.InvokeAsync(() => 
                {
                    MessageScrollViewer.UpdateLayout();
                    MessageScrollViewer.ScrollToBottom();
                }, System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        private async void ConvList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ConvList.SelectedItem is Conversation selected)
            {
                SelectedConversation = selected;
                
                // Clear old messages
                CurrentMessages.Clear();

                // Load new messages
                var messages = await _chatController.GetRecentMessagesAsync(selected.Id);
                foreach(var m in messages)
                {
                    CurrentMessages.Add(m);
                }
            }
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Subscribe to real-time events via Controller
            _chatController.OnMessageReceived += ChatController_OnMessageReceived;
            _chatController.OnUserTyping += ChatController_OnUserTyping;
            _chatController.OnUserOnlineStatusChanged += ChatController_OnUserOnlineStatusChanged;

            if (App.CurrentUser != null)
            {
                await _chatController.ConnectRealtimeAsync(App.CurrentUser.Id);
                
                // 1. Tải danh sách nhóm chat từ DB
                var convos = await _chatController.GetUserConversationsAsync(App.CurrentUser.Id);
                foreach(var c in convos)
                {
                    Conversations.Add(c);
                }

                // 2. Mặc định chọn nhóm đầu tiên nếu có
                if (Conversations.Count > 0)
                {
                    SelectedConversation = Conversations[0];
                    var messages = await _chatController.GetRecentMessagesAsync(SelectedConversation.Id);
                    foreach(var m in messages)
                    {
                        CurrentMessages.Add(m);
                    }
                }
            }
        }

        private async void MainWindow_Closed(object? sender, EventArgs e)
        {
            await _chatController.DisconnectRealtimeAsync();
        }

        // ── SignalR Event Handlers via Controller ──────────────
        private void ChatController_OnMessageReceived(int conversationId, int senderId, string content)
        {
            Dispatcher.Invoke(() =>
            {
                // Nếu tin nhắn thuộc về Conversation đang mở -> Add vào ObservableCollection
                if (SelectedConversation != null && SelectedConversation.Id == conversationId)
                {
                    CurrentMessages.Add(new Message 
                    { 
                        ConversationId = conversationId, 
                        SenderId = senderId, 
                        Content = content,
                        CreatedAt = DateTime.UtcNow.AddHours(7)
                    });
                    
                    // UI sẽ tự động update nếu bạn xài ListBox/ItemsControl binding với CurrentMessages
                }
            });
        }

        private void ChatController_OnUserTyping(int conversationId, string userName)
        {
            Dispatcher.Invoke(() =>
            {
                if (SelectedConversation != null && SelectedConversation.Id == conversationId)
                {
                    // Logic hiện UI "đang nhập..."
                    // Ví dụ: lblTypingIndicator.Visibility = Visibility.Visible;
                }
            });
        }

        private void ChatController_OnUserOnlineStatusChanged(int userId, bool isOnline)
        {
            Dispatcher.Invoke(() =>
            {
                // Logic tìm tới User trong danh sách Coversations -> Update dot màu xanh/xám
                // Ví dụ: user.IsOnline = isOnline; (gọi OnPropertyChanged để cập nhật UI)
            });
        }

        // ── Actions ───────────────────────────────────────────
        private void BtnGroupSettings_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedConversation == null)
            {
                MessageBox.Show("Please select a conversation first.");
                return;
            }

            if (SelectedConversation.Type != Models.Entities.ConversationType.Group)
            {
                MessageBox.Show("Settings are only available for group conversations.");
                return;
            }

            var groupSettingsWindow = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<Views.GroupSettingsWindow>(App.ServiceProvider);
            groupSettingsWindow.SetConversation(SelectedConversation);
            groupSettingsWindow.Owner = this;
            
            bool? result = groupSettingsWindow.ShowDialog();
            if (result == true)
            {
                // If user leaves or disbands group, remove conversation from UI
                Conversations.Remove(SelectedConversation);
                SelectedConversation = null;
                CurrentMessages.Clear();
            }
        }
        
        private async void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            await SendMessageAction();
        }

        private async void TxtInputMessage_KeyDown(object sender, KeyEventArgs e)
        {
            // Bấm Enter để gửi, nhấn Shift+Enter để xuống dòng
            if (e.Key == Key.Enter && Keyboard.Modifiers != ModifierKeys.Shift)
            {
                e.Handled = true; // Chặn tiếng bíp
                await SendMessageAction();
            }
        }

        private async System.Threading.Tasks.Task SendMessageAction()
        {
            if (App.CurrentUser == null || SelectedConversation == null) return;
            
            string txtContent = txtInputMessage.Text;
            if (string.IsNullOrWhiteSpace(txtContent) || txtContent == "Nhập tin nhắn...") return;

            var result = await _chatController.SendTextMessageAsync(SelectedConversation.Id, App.CurrentUser.Id, txtContent);
            if (result.Success && result.SentMessage != null)
            {
                // Thêm ngay tin nhắn của mình vào list
                CurrentMessages.Add(result.SentMessage);
                
                // Clear textbox
                txtInputMessage.Text = "";
            }
            else
            {
                MessageBox.Show(result.Message, "Error Sending Message", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnSelectFile_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser == null) return;
            if (SelectedConversation == null)
            {
                MessageBox.Show("Vui lòng chọn một cuộc trò chuyện trước khi gửi file đính kèm.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select a file to send";
            openFileDialog.Filter = "All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                var result = await _chatController.SendAttachmentMessageAsync(SelectedConversation.Id, App.CurrentUser.Id, openFileDialog.FileName, MessageType.File);
                if (result.Success && result.SentMessage != null)
                {
                    CurrentMessages.Add(result.SentMessage);
                }
                else
                {
                    MessageBox.Show(result.Message, "Error Sending File", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void BtnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser == null) return;
            if (SelectedConversation == null)
            {
                MessageBox.Show("Vui lòng chọn một cuộc trò chuyện trước khi đính kèm hình ảnh.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select an image to send";
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                var result = await _chatController.SendAttachmentMessageAsync(SelectedConversation.Id, App.CurrentUser.Id, openFileDialog.FileName, MessageType.Image);
                if (result.Success && result.SentMessage != null)
                {
                    CurrentMessages.Add(result.SentMessage);
                }
                else
                {
                    MessageBox.Show(result.Message, "Error Sending Image", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void BtnNewChat_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser == null) return;

            var createGroupWin = new Views.CreateGroupWindow();
            createGroupWin.Owner = this;
            if (createGroupWin.ShowDialog() == true)
            {
                string groupName = createGroupWin.GroupName;

                var result = await _chatController.CreateGroupAsync(App.CurrentUser.Id, groupName, new System.Collections.Generic.List<int>());
                if (result.Success && result.Group != null)
                {
                    Conversations.Add(result.Group);
                    SelectedConversation = result.Group;
                }
                else
                {
                    MessageBox.Show(result.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // ── Placeholder UX ─────────────────────────────────────
        private void TxtInputMessage_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtInputMessage.Text == "Nhập tin nhắn...")
            {
                txtInputMessage.Text = "";
                txtInputMessage.Foreground = new SolidColorBrush(Colors.Black);
            }
        }

        private void TxtInputMessage_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtInputMessage.Text))
            {
                txtInputMessage.Text = "Nhập tin nhắn...";
                txtInputMessage.Foreground = new SolidColorBrush(Color.FromRgb(154, 154, 154)); // #9A9A9A
            }
        }

        // ── Search Box UX & Filtering ──────────────────────────
        private void TxtSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtSearch.Text == "Search...")
            {
                txtSearch.Text = "";
                txtSearch.Foreground = new SolidColorBrush(Color.FromRgb(26, 26, 26)); // #1A1A1A
            }
        }

        private void TxtSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                txtSearch.Text = "Search...";
                txtSearch.Foreground = new SolidColorBrush(Color.FromRgb(154, 154, 154)); // #9A9A9A
            }
        }

        private void TxtSearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // Trigger filter refresh
            if (ConversationsView != null)
            {
                ConversationsView.Refresh();
            }
        }

        private bool FilterConversation(object obj)
        {
            if (obj is Conversation conv)
            {
                string searchText = txtSearch.Text;
                if (string.IsNullOrWhiteSpace(searchText) || searchText == "Search...") 
                    return true;
                
                if (!string.IsNullOrEmpty(conv.Name))
                {
                    return conv.Name.Contains(searchText, System.StringComparison.OrdinalIgnoreCase);
                }
            }
            return false;
        }
    }
}