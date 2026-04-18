using System;

namespace Chat_Group_System.Models.Entities
{
    /// <summary>
    /// Lưu ai đã đọc tin nhắn nào — nguồn dữ liệu cho read receipt ✓✓.
    /// Mỗi record = 1 user đã đọc 1 message tại thời điểm ReadAt.
    /// </summary>
    public class MessageRead
    {
        public int Id { get; set; }

        // ── Foreign Keys ──────────────────────────────────────
        public int MessageId { get; set; }
        public int UserId { get; set; }

        /// <summary>Thời điểm user mở/đọc message này.</summary>
        public DateTime ReadAt { get; set; } = Chat_Group_System.Helpers.TimeHelper.NowVN;

        // ── Navigation ────────────────────────────────────────
        public Message Message { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}

