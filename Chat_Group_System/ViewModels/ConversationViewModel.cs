using System;
using System.Collections.ObjectModel;
using System.Linq;
using Chat_Group_System.Models.Entities;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Chat_Group_System.ViewModels
{
    public partial class ConversationViewModel : ObservableObject
    {
        [ObservableProperty]
        private int _id;

        [ObservableProperty]
        private string? _name;

        [ObservableProperty]
        private bool _isGroup;

        [ObservableProperty]
        private string? _avatarUrl;

        [ObservableProperty]
        private string? _lastMessagePreview;

        [ObservableProperty]
        private DateTime? _updatedAt;

        [ObservableProperty]
        private bool _isOnline; // For UI later

        [ObservableProperty]
        private int _unreadCount; // For UI later

        [ObservableProperty]
        private bool _isTyping;

        [ObservableProperty]
        private string? _typingMessage;

        public ObservableCollection<MessageViewModel> Messages { get; } = new();

        public ConversationViewModel(Conversation conversation, int currentUserId = -1)
        {
            Id = conversation.Id;
            IsGroup = conversation.Type == ConversationType.Group;
            AvatarUrl = conversation.AvatarUrl;
            LastMessagePreview = conversation.LastMessagePreview;
            UpdatedAt = conversation.LastMessageAt;

            if (IsGroup || string.IsNullOrEmpty(conversation.Name))
            {
                Name = conversation.Name;
            }

            if (!IsGroup && conversation.Members != null && conversation.Members.Any() && currentUserId > 0)
            {
                var otherUser = conversation.Members.FirstOrDefault(m => m.UserId != currentUserId)?.User;
                if (otherUser != null)
                {
                    Name = otherUser.DisplayName;
                    if (string.IsNullOrEmpty(AvatarUrl))
                    {
                        AvatarUrl = otherUser.AvatarUrl;
                    }
                }
            }
            else if (!IsGroup && string.IsNullOrEmpty(Name))
            {
                Name = "Chat";
            }
        }
    }
}