using System;
using System.Collections.Generic;

namespace Chat_Group_System.Models.Entities
{
    /// <summary>
    /// Một tin nhắn trong conversation.
    /// Hỗ trợ text, file và image — phân biệt bằng MessageType.
    /// </summary>
    public class Message
    {
        public int Id { get; set; }

        // ── Foreign Keys ──────────────────────────────────────
        public int ConversationId { get; set; }
        public int SenderId { get; set; }

        /// <summary>
        /// Reply-to: nếu tin nhắn này đang reply một tin khác.
        /// </summary>
        public int? ReplyToMessageId { get; set; }

        // ── Content ───────────────────────────────────────────
        /// <summary>Nội dung text (null nếu là file/image thuần).</summary>
        public string? Content { get; set; }

        /// <summary>Loại tin nhắn: Text, File, Image, System.</summary>
        public MessageType Type { get; set; } = MessageType.Text;

        // ── Read Status ───────────────────────────────────────
        /// <summary>
        /// Trạng thái gửi — dùng cho read receipt icon dưới bubble.
        ///   Sent      → ✓  (đã gửi lên server)
        ///   Delivered → ✓✓ xám (ít nhất 1 thành viên đã nhận)
        ///   Read      → ✓✓ xanh (tất cả đã đọc)
        /// </summary>
        public MessageStatus Status { get; set; } = MessageStatus.Sent;

        // ── Soft Delete ───────────────────────────────────────
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // ── Timestamps ────────────────────────────────────────
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? EditedAt { get; set; }

        // ── Navigation ────────────────────────────────────────
        public Conversation Conversation { get; set; } = null!;
        public User Sender { get; set; } = null!;
        public Message? ReplyToMessage { get; set; }

        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
        public ICollection<MessageRead> ReadReceipts { get; set; } = new List<MessageRead>();
    }

    public enum MessageType
    {
        Text = 0,
        File = 1,
        Image = 2,
        /// <summary>Tin hệ thống: "Nguyen Van A đã tham gia nhóm"</summary>
        System = 3
    }

    public enum MessageStatus
    {
        /// <summary>✓ — Đã lưu xuống server</summary>
        Sent = 0,
        /// <summary>✓✓ xám — Ít nhất 1 recipient đã nhận</summary>
        Delivered = 1,
        /// <summary>✓✓ xanh — Tất cả recipients đã đọc</summary>
        Read = 2
    }
}
