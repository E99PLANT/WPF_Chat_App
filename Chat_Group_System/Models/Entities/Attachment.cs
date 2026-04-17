using System;

namespace Chat_Group_System.Models.Entities
{
    /// <summary>
    /// File / ảnh đính kèm trong một tin nhắn.
    /// Một Message có thể có nhiều Attachments (multi-file upload).
    /// </summary>
    public class Attachment
    {
        public int Id { get; set; }

        // ── Foreign Key ───────────────────────────────────────
        public int MessageId { get; set; }

        // ── File Info ─────────────────────────────────────────
        /// <summary>Tên file gốc người dùng upload (vd: "Report_Q1.pdf").</summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>Tên file trên server sau khi lưu (GUID để tránh trùng).</summary>
        public string StoredFileName { get; set; } = string.Empty;

        /// <summary>Đường dẫn / URL để download hoặc preview.</summary>
        public string FileUrl { get; set; } = string.Empty;

        /// <summary>MIME type: "image/png", "application/pdf", "video/mp4"...</summary>
        public string MimeType { get; set; } = string.Empty;

        /// <summary>Kích thước file tính bằng bytes — hiện "2.4 MB" trong bubble.</summary>
        public long SizeBytes { get; set; }

        /// <summary>Loại attachment để render đúng UI: Image, File, Video.</summary>
        public AttachmentType Type { get; set; } = AttachmentType.File;

        /// <summary>
        /// Nếu là ảnh: thumbnail URL (resize nhỏ để preview inline, click mới xem full).
        /// </summary>
        public string? ThumbnailUrl { get; set; }

        /// <summary>
        /// Width/Height nếu là ảnh — giúp WPF tính đúng aspect ratio trước khi load.
        /// </summary>
        public int? ImageWidth { get; set; }
        public int? ImageHeight { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow.AddHours(7);

        // ── Navigation ────────────────────────────────────────
        public Message Message { get; set; } = null!;
    }

    public enum AttachmentType
    {
        File = 0,
        Image = 1,
        Video = 2
    }
}
