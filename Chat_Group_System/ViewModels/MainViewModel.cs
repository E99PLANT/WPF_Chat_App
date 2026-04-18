using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Threading;
using Chat_Group_System.Models.Entities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Chat_Group_System.Controllers;

namespace Chat_Group_System.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ChatController _chatController;
        private DispatcherTimer? _typingTimer;
        private DateTime _lastTypingSentTime = DateTime.MinValue;

        [ObservableProperty]
        private ObservableCollection<ConversationViewModel> _conversations = new();

        [ObservableProperty]
        private ICollectionView _filteredConversations;

        [ObservableProperty]
        private ObservableCollection<MessageViewModel> _currentMessages = new();

        [ObservableProperty]
        private ConversationViewModel? _selectedConversation;

        [ObservableProperty]
        private string _searchText = "Search...";

        [ObservableProperty]
        private string _inputText = "Nhập tin nhắn...";

        [ObservableProperty]
        private bool _isTyping;

        [ObservableProperty]
        private string _typingMessage = string.Empty;

        public MainViewModel(ChatController chatController)
        {
            _chatController = chatController;
            _filteredConversations = CollectionViewSource.GetDefaultView(_conversations);
            FilteredConversations.Filter = FilterConversations;

            // Subscribe to SignalR events
            _chatController.OnMessageReceived += ChatController_OnMessageReceived;
            _chatController.OnUserTyping += ChatController_OnUserTyping;
            _chatController.OnUserOnlineStatusChanged += ChatController_OnUserOnlineStatusChanged;
        }

        public void Cleanup()
        {
            _chatController.OnMessageReceived -= ChatController_OnMessageReceived;
            _chatController.OnUserTyping -= ChatController_OnUserTyping;
            _chatController.OnUserOnlineStatusChanged -= ChatController_OnUserOnlineStatusChanged;
        }

        private void ChatController_OnMessageReceived(int convId, int senderId, string content)
        {
            // Update UI on the main thread
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (SelectedConversation != null && SelectedConversation.Id == convId)
                {
                    if (App.CurrentUser != null && senderId == App.CurrentUser.Id)
                    {
                        return; // Ignore self
                    }

                    if (content.StartsWith("[ATTACHMENT]"))
                    {
                        string json = content.Substring("[ATTACHMENT]".Length);
                        try
                        {
                            var data = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(json);
                            var msg = new Message
                            {
                                Id = data.TryGetProperty("Id", out var idProp) ? idProp.GetInt32() : 0,
                                ConversationId = convId,
                                SenderId = senderId,
                                Type = data.TryGetProperty("Type", out var typeProp) ? (MessageType)typeProp.GetInt32() : MessageType.File,
                                CreatedAt = Chat_Group_System.Helpers.TimeHelper.NowVN,
                                Content = ""
                            };

                            var fileName = data.TryGetProperty("FileName", out var fnProp) ? fnProp.GetString() : null;
                            var fileSize = data.TryGetProperty("FileSize", out var fsProp) ? fsProp.GetInt64() : 0;
                            var filePath = data.TryGetProperty("FilePath", out var fpProp) ? fpProp.GetString() : null;

                            if (fileName != null)
                            {
                                msg.Attachments.Add(new Chat_Group_System.Models.Entities.Attachment
                                {
                                    FileName = fileName,
                                    SizeBytes = fileSize,
                                    FileUrl = filePath ?? ""
                                });
                            }
                            CurrentMessages.Add(new MessageViewModel(msg));
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to parse attachment json: {ex.Message}");
                        }
                    }
                    else
                    {
                        // Update active conversation
                        CurrentMessages.Add(new MessageViewModel(new Message { ConversationId = convId, SenderId = senderId, Content = content, Type = MessageType.Text, CreatedAt = Chat_Group_System.Helpers.TimeHelper.NowVN }));
                    }
                }
                else
                {
                    // Increment Unread Badge for background conversation
                    var targetConv = default(ConversationViewModel);
                    foreach (var conv in Conversations)
                    {
                        if (conv.Id == convId)
                        {
                            targetConv = conv;
                            break;
                        }
                    }

                    if (targetConv != null)
                    {
                        targetConv.UnreadCount++;
                        targetConv.LastMessagePreview = content.StartsWith("[ATTACHMENT]") ? "[File Attached]" : content;
                    }
                }
            });
        }

        private void ChatController_OnUserTyping(int convId, string userName)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (SelectedConversation != null && SelectedConversation.Id == convId)
                {
                    IsTyping = true;
                    TypingMessage = $"{userName} đang gõ...";

                    _typingTimer?.Stop();
                    if (_typingTimer == null)
                    {
                        _typingTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
                        _typingTimer.Tick += (s, e) =>
                        {
                            IsTyping = false;
                            TypingMessage = string.Empty;
                            _typingTimer.Stop();
                        };
                    }
                    _typingTimer.Start();
                }
            });
        }

        private void ChatController_OnUserOnlineStatusChanged(int userId, bool isOnline)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                // To do: Update online status in conversation list members
            });
        }

        partial void OnSelectedConversationChanged(ConversationViewModel? oldValue, ConversationViewModel? newValue)
        {
            if (oldValue != null)
            {
                _ = _chatController.LeaveGroupAsync(oldValue.Id);
            }
            if (newValue != null)
            {
                newValue.UnreadCount = 0; // Clear unread 
                _ = _chatController.JoinGroupAsync(newValue.Id);
                // Ideally, load recent messages here
            }
        }

        partial void OnInputTextChanged(string value)
        {
            if (SelectedConversation != null && !string.IsNullOrWhiteSpace(value) && value != "Nhập tin nhắn...")
            {
                if ((DateTime.UtcNow - _lastTypingSentTime).TotalMilliseconds > 1500)
                {
                    _lastTypingSentTime = DateTime.UtcNow;
                    var userName = App.CurrentUser?.DisplayName ?? "Someone";
                    _ = _chatController.NotifyTypingAsync(SelectedConversation.Id, userName);
                }
            }
        }

        partial void OnSearchTextChanged(string value)
        {
            FilteredConversations.Refresh();
        }

        private bool FilterConversations(object obj)
        {
            if (string.IsNullOrWhiteSpace(SearchText) || SearchText == "Search...")
                return true;

            if (obj is ConversationViewModel conv && conv.Name != null)
                return conv.Name.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase);

            return false;
        }
    }
}