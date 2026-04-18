using System;
using System.Collections.ObjectModel;
using Chat_Group_System.Models.Entities;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Chat_Group_System.ViewModels
{
    public partial class MessageViewModel : ObservableObject
    {
        [ObservableProperty]
        private int _id;

        [ObservableProperty]
        private int _conversationId;

        [ObservableProperty]
        private int _senderId;

        [ObservableProperty]
        private string? _content;

        [ObservableProperty]
        private string? _senderName;

        [ObservableProperty]
        private MessageType _type;

        [ObservableProperty]
        private DateTime _createdAt;

        [ObservableProperty]
        private MessageStatus _status;

        public ObservableCollection<Attachment> Attachments { get; } = new();

        public bool IsSelf => App.CurrentUser != null && SenderId == App.CurrentUser.Id;

        public MessageViewModel(Message message)
        {
            Id = message.Id;
            ConversationId = message.ConversationId;
            SenderId = message.SenderId;
            SenderName = message.Sender?.DisplayName ?? "Unknown User";
            Content = message.Content;
            Type = message.Type;
            CreatedAt = message.CreatedAt;
            Status = message.Status;
            
            foreach (var attachment in message.Attachments)
            {
                Attachments.Add(attachment);
            }
        }
    }
}