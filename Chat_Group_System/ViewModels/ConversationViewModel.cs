using System;
using System.Collections.ObjectModel;
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

        public ConversationViewModel(Conversation conversation)
        {
            Id = conversation.Id;
            Name = conversation.Name;
            IsGroup = conversation.Type == ConversationType.Group;
            AvatarUrl = conversation.AvatarUrl;
            LastMessagePreview = conversation.LastMessagePreview;
            UpdatedAt = conversation.LastMessageAt;
        }
    }
}