using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Threading;
using Chat_Group_System.Models.Entities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Chat_Group_System.Controllers;
using Microsoft.Extensions.DependencyInjection;

namespace Chat_Group_System.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ChatController _chatController;
        private DispatcherTimer? _typingDebounceTimer;
        private DateTime _lastTypingSentTime = DateTime.MinValue;
        private readonly System.Collections.Generic.List<string> _typingUsers = new();

        [ObservableProperty]
        private ObservableCollection<ConversationViewModel> _conversations = new();

        [ObservableProperty]
        private ICollectionView _filteredConversations;

        [ObservableProperty]
        private ObservableCollection<MessageViewModel> _currentMessages = new();

        [ObservableProperty]
        private ConversationViewModel? _selectedConversation;

        [ObservableProperty]
        private bool _canSendMessage = true;

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
            _chatController.OnAddedToGroup += ChatController_OnAddedToGroup;
        }

        public void Cleanup()
        {
            _chatController.OnMessageReceived -= ChatController_OnMessageReceived;
            _chatController.OnUserTyping -= ChatController_OnUserTyping;
            _chatController.OnUserOnlineStatusChanged -= ChatController_OnUserOnlineStatusChanged;
            _chatController.OnAddedToGroup -= ChatController_OnAddedToGroup;
        }

        private void ChatController_OnAddedToGroup(int convId)
        {
            System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                if (App.CurrentUser == null) return;
                
                using var scope = App.ServiceProvider.CreateScope();
                var scopedConversationService = scope.ServiceProvider.GetRequiredService<Chat_Group_System.Services.IConversationService>();
                
                var convos = await scopedConversationService.GetUserConversationsAsync(App.CurrentUser.Id);
                var newConvo = System.Linq.Enumerable.FirstOrDefault(convos, c => c.Id == convId);

                if (newConvo != null)
                {
                    // Check if already in list
                    if (!System.Linq.Enumerable.Any(Conversations, c => c.Id == convId))
                    {
                        var vm = new ConversationViewModel(newConvo, App.CurrentUser.Id);
                        vm.UnreadCount = 0; // Or whatever you prefer
                        Conversations.Add(vm);
                        
                        // Let the real time signal R know to listen
                        await _chatController.JoinGroupAsync(convId);
                    }
                }
            });
        }

        private void ChatController_OnMessageReceived(int convId, int senderId, string content)
        {
            // Update UI on the main thread
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                bool isSystem = content.StartsWith("[SYSTEM]");
                
                if (SelectedConversation != null && SelectedConversation.Id == convId)
                {
                    if (App.CurrentUser != null && senderId == App.CurrentUser.Id && !isSystem)
                    {
                        return; // Ignore self
                    }

                    if (content.StartsWith("[SYSTEM]"))
                    {
                        string json = content.Substring("[SYSTEM]".Length);
                        try
                        {
                            var data = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(json);
                            var msg = new Message
                            {
                                Id = data.TryGetProperty("Id", out var idProp) ? idProp.GetInt32() : 0,
                                ConversationId = convId,
                                SenderId = senderId,
                                Type = MessageType.System,
                                CreatedAt = Chat_Group_System.Helpers.TimeHelper.NowVN,
                                Content = data.TryGetProperty("Content", out var contentProp) ? contentProp.GetString() : "System Message"
                            };
                            CurrentMessages.Add(new MessageViewModel(msg));

                            // Refresh Send permissions when System messages occur (kick, leave, etc.)
                            System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
                            {
                                using var scope = App.ServiceProvider.CreateScope();
                                var scopedController = scope.ServiceProvider.GetRequiredService<ChatController>();

                                var members = await scopedController.GetGroupMembersAsync(convId);
                                CanSendMessage = members.Any(m => m.UserId == App.CurrentUser?.Id);
                            });
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to parse system message json: {ex.Message}");
                        }
                    }
                    else if (content.StartsWith("[ATTACHMENT]"))
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
                            var msgViewModel = new MessageViewModel(msg);
                            CurrentMessages.Add(msgViewModel);

                            // Fetch SenderName asynchronously
                            System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
                            {
                                using var scope = App.ServiceProvider.CreateScope();
                                var userController = scope.ServiceProvider.GetRequiredService<UserController>();
                                var senderUser = await userController.GetUserByIdAsync(senderId);
                                if (senderUser != null)
                                {
                                    msgViewModel.SenderName = senderUser.DisplayName;
                                }
                            });
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to parse attachment json: {ex.Message}");
                        }
                    }
                    else
                    {
                        // Update active conversation
                        var newMsg = new Message { ConversationId = convId, SenderId = senderId, Content = content, Type = MessageType.Text, CreatedAt = Chat_Group_System.Helpers.TimeHelper.NowVN };
                        var msgViewModel = new MessageViewModel(newMsg);
                        CurrentMessages.Add(msgViewModel);

                        // Fetch SenderName asynchronously
                        System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
                        {
                            using var scope = App.ServiceProvider.CreateScope();
                            var userController = scope.ServiceProvider.GetRequiredService<UserController>();
                            var senderUser = await userController.GetUserByIdAsync(senderId);
                            if (senderUser != null)
                            {
                                msgViewModel.SenderName = senderUser.DisplayName;
                            }
                        });
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
                        targetConv.LastMessagePreview = content.StartsWith("[SYSTEM]") ? "Tin nhắn hệ thống" : 
                            (content.StartsWith("[ATTACHMENT]") ? "[File Attached]" : content);
                    }
                }
            });
        }

        private void ChatController_OnUserTyping(int convId, string userName, bool isTyping)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (SelectedConversation != null && SelectedConversation.Id == convId)
                {
                    if (isTyping)
                    {
                        if (!_typingUsers.Contains(userName))
                            _typingUsers.Add(userName);
                    }
                    else
                    {
                        _typingUsers.Remove(userName);
                    }

                    UpdateTypingMessage();
                }
            });
        }

        private void UpdateTypingMessage()
        {
            if (_typingUsers.Count == 0)
            {
                IsTyping = false;
                TypingMessage = string.Empty;
            }
            else
            {
                IsTyping = true;
                if (_typingUsers.Count == 1)
                {
                    TypingMessage = $"{_typingUsers[0]} đang gõ...";
                }
                else if (_typingUsers.Count == 2)
                {
                    TypingMessage = $"{_typingUsers[0]} và {_typingUsers[1]} đang gõ...";
                }
                else if (_typingUsers.Count == 3)
                {
                    TypingMessage = $"{_typingUsers[0]}, {_typingUsers[1]} và {_typingUsers[2]} đang gõ...";
                }
                else
                {
                    string firstThree = string.Join(", ", _typingUsers.GetRange(0, 3));
                    TypingMessage = $"{firstThree}... đang gõ...";
                }
            }
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
            if (newValue != null)
            {
                newValue.UnreadCount = 0; // Clear unread 
                // Group is already joined on load or creation, but joining again is safe and idempotent in SignalR
                _ = _chatController.JoinGroupAsync(newValue.Id);
            }
            
            // Tắt trạng thái gõ khi đổi nhóm
            _typingUsers.Clear();
            UpdateTypingMessage();
        }

        partial void OnInputTextChanged(string value)
        {
            if (SelectedConversation == null) return;

            bool isCurrentlyTyping = !string.IsNullOrWhiteSpace(value) && value != "Nhập tin nhắn...";
            
            if (isCurrentlyTyping)
            {
                if ((DateTime.UtcNow - _lastTypingSentTime).TotalMilliseconds > 1500)
                {
                    _lastTypingSentTime = DateTime.UtcNow;
                    var userName = App.CurrentUser?.DisplayName ?? "Someone";
                    _ = _chatController.NotifyTypingAsync(SelectedConversation.Id, userName, true);
                }

                // Reset stop typing timer
                if (_typingDebounceTimer == null)
                {
                    _typingDebounceTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2.5) };
                    _typingDebounceTimer.Tick += (s, e) =>
                    {
                        _typingDebounceTimer.Stop();
                        var name = App.CurrentUser?.DisplayName ?? "Someone";
                        if (SelectedConversation != null)
                            _ = _chatController.NotifyTypingAsync(SelectedConversation.Id, name, false);
                    };
                }
                _typingDebounceTimer.Stop();
                _typingDebounceTimer.Start();
            }
            else
            {
                // Ngay lập tức báo ngưng gõ nếu chuỗi rỗng
                _typingDebounceTimer?.Stop();
                var userName = App.CurrentUser?.DisplayName ?? "Someone";
                _ = _chatController.NotifyTypingAsync(SelectedConversation.Id, userName, false);
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