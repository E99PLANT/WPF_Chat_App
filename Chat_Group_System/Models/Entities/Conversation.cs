using System;
using System.Collections.Generic;

namespace Chat_Group_System.Models.Entities
{
    /// <summary>
    /// Một cuộc trò chuyện — có thể là Group Chat hoặc Direct Message (DM).
    /// </summary>
    public class Conversation
    {
        public int Id { get; set; }

        /// <summary>Tên group (null nếu là DM — tự generate từ tên 2 người).</summary>
        public string? Name { get; set; }

        /// <summary>Loại conversation. Group = nhóm nhiều người, DM = nhắn riêng 1-1.</summary>
        public ConversationType Type { get; set; } = ConversationType.Group;

        /// <summary>Ảnh đại diện group (null nếu là DM).</summary>
        public string? AvatarUrl { get; set; }

        /// <summary>Mô tả group (optional).</summary>
        public string? Description { get; set; }

        /// <summary>Tin nhắn cuối — cache để hiện preview trong sidebar mà không cần JOIN nặng.</summary>
        public string? LastMessagePreview { get; set; }

        public DateTime? LastMessageAt { get; set; }

        public DateTime CreatedAt { get; set; } = Chat_Group_System.Helpers.TimeHelper.NowVN;

        public bool IsActive { get; set; } = true;

        // ── Navigation ─────────────────────────────────────────
        public ICollection<ConversationMember> Members { get; set; } = new List<ConversationMember>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }

    public enum ConversationType
    {
        Group = 0,
        DirectMessage = 1
    }
}

