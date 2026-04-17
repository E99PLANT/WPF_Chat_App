using System;

namespace Chat_Group_System.Models.Entities
{
    /// <summary>
    /// Bảng trung gian User ↔ Conversation (Many-to-Many).
    /// Track role trong group + thời điểm join.
    /// </summary>
    public class ConversationMember
    {
        public int Id { get; set; }

        // ── Foreign Keys ──────────────────────────────────────
        public int ConversationId { get; set; }
        public int UserId { get; set; }

        /// <summary>Role trong conversation: "Admin" | "Member"</summary>
        public string Role { get; set; } = "Member";

        /// <summary>Thời điểm được thêm vào group.</summary>
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow.AddHours(7);

        /// <summary>
        /// Số tin nhắn chưa đọc trong conversation này — cache để hiện badge đỏ.
        /// Reset về 0 khi user mở conversation.
        /// </summary>
        public int UnreadCount { get; set; } = 0;

        /// <summary>User có bị mute notification trong group này không.</summary>
        public bool IsMuted { get; set; } = false;

        /// <summary>Soft-leave: user rời group nhưng vẫn giữ lịch sử.</summary>
        public bool HasLeft { get; set; } = false;

        public DateTime? LeftAt { get; set; }

        // ── Navigation ────────────────────────────────────────
        public Conversation Conversation { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
