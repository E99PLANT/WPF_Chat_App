using System;
using System.Collections.Generic;

namespace Chat_Group_System.Models.Entities
{
    /// <summary>
    /// Tài khoản người dùng — kiêm luôn auth (email/password) và profile (avatar, online status).
    /// </summary>
    public class User
    {
        public int Id { get; set; }

        // ── Auth ─────────────────────────────────────────────
        /// <summary>Email đăng nhập, unique.</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>Password đã hash (BCrypt).</summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>Role: "Admin" | "Member"</summary>
        public string Role { get; set; } = "Member";

        public bool IsActive { get; set; } = true;

        // ── Profile ──────────────────────────────────────────
        /// <summary>Tên hiển thị trong chat.</summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>Đường dẫn avatar (local path hoặc URL).</summary>
        public string? AvatarUrl { get; set; }

        // ── Online Status ─────────────────────────────────────
        /// <summary>Hiện tại có online không → dùng cho dot xanh trong sidebar.</summary>
        public bool IsOnline { get; set; } = false;

        /// <summary>Lần cuối online — hiện "Last seen 5 minutes ago".</summary>
        public DateTime? LastSeenAt { get; set; }

        // ── Timestamps ────────────────────────────────────────
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ── Navigation ────────────────────────────────────────
        public ICollection<ConversationMember> ConversationMembers { get; set; } = new List<ConversationMember>();
        public ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public ICollection<MessageRead> MessageReads { get; set; } = new List<MessageRead>();
    }
}
