using Chat_Group_System.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chat_Group_System.Models.Data
{
    public class NexChatDbContext : DbContext
    {
        public NexChatDbContext(DbContextOptions<NexChatDbContext> options) : base(options) { }

        // ── DbSets ────────────────────────────────────────────
        public DbSet<User> Users => Set<User>();
        public DbSet<Conversation> Conversations => Set<Conversation>();
        public DbSet<ConversationMember> ConversationMembers => Set<ConversationMember>();
        public DbSet<Message> Messages => Set<Message>();
        public DbSet<MessageRead> MessageReads => Set<MessageRead>();
        public DbSet<Attachment> Attachments => Set<Attachment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── User ──────────────────────────────────────────
            modelBuilder.Entity<User>(e =>
            {
                e.HasKey(u => u.Id);
                e.HasIndex(u => u.Email).IsUnique();
                e.Property(u => u.Email).HasMaxLength(256).IsRequired();
                e.Property(u => u.PasswordHash).IsRequired();
                e.Property(u => u.DisplayName).HasMaxLength(100).IsRequired();
                e.Property(u => u.Role).HasMaxLength(20).HasDefaultValue("Member");
                e.Property(u => u.AvatarUrl).HasMaxLength(500);
            });

            // ── Conversation ──────────────────────────────────
            modelBuilder.Entity<Conversation>(e =>
            {
                e.HasKey(c => c.Id);
                e.Property(c => c.Name).HasMaxLength(200);
                e.Property(c => c.Description).HasMaxLength(500);
                e.Property(c => c.LastMessagePreview).HasMaxLength(300);
                e.Property(c => c.AvatarUrl).HasMaxLength(500);
                e.Property(c => c.Type)
                    .HasConversion<string>()
                    .HasMaxLength(20);
            });

            // ── ConversationMember ────────────────────────────
            modelBuilder.Entity<ConversationMember>(e =>
            {
                e.HasKey(cm => cm.Id);

                // Unique: mỗi user chỉ là member của 1 conversation 1 lần
                e.HasIndex(cm => new { cm.ConversationId, cm.UserId }).IsUnique();

                e.Property(cm => cm.Role).HasMaxLength(20).HasDefaultValue("Member");

                // Conversation → Members
                e.HasOne(cm => cm.Conversation)
                    .WithMany(c => c.Members)
                    .HasForeignKey(cm => cm.ConversationId)
                    .OnDelete(DeleteBehavior.Cascade);

                // User → ConversationMembers
                e.HasOne(cm => cm.User)
                    .WithMany(u => u.ConversationMembers)
                    .HasForeignKey(cm => cm.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ── Message ───────────────────────────────────────
            modelBuilder.Entity<Message>(e =>
            {
                e.HasKey(m => m.Id);
                e.Property(m => m.Content).HasMaxLength(4000);
                e.Property(m => m.Type)
                    .HasConversion<string>()
                    .HasMaxLength(20);
                e.Property(m => m.Status)
                    .HasConversion<string>()
                    .HasMaxLength(20);

                // Message → Conversation
                e.HasOne(m => m.Conversation)
                    .WithMany(c => c.Messages)
                    .HasForeignKey(m => m.ConversationId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Message → Sender (User)
                e.HasOne(m => m.Sender)
                    .WithMany(u => u.SentMessages)
                    .HasForeignKey(m => m.SenderId)
                    .OnDelete(DeleteBehavior.Restrict); // không xóa user kéo theo xóa message

                // Reply-to self-reference — NoAction để tránh SQL Server cascade cycle
                e.HasOne(m => m.ReplyToMessage)
                    .WithMany()
                    .HasForeignKey(m => m.ReplyToMessageId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // ── MessageRead ───────────────────────────────────
            modelBuilder.Entity<MessageRead>(e =>
            {
                e.HasKey(mr => mr.Id);

                // Unique: mỗi user chỉ "đọc" 1 message 1 lần
                e.HasIndex(mr => new { mr.MessageId, mr.UserId }).IsUnique();

                e.HasOne(mr => mr.Message)
                    .WithMany(m => m.ReadReceipts)
                    .HasForeignKey(mr => mr.MessageId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(mr => mr.User)
                    .WithMany(u => u.MessageReads)
                    .HasForeignKey(mr => mr.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // ── Attachment ────────────────────────────────────
            modelBuilder.Entity<Attachment>(e =>
            {
                e.HasKey(a => a.Id);
                e.Property(a => a.FileName).HasMaxLength(255).IsRequired();
                e.Property(a => a.StoredFileName).HasMaxLength(255).IsRequired();
                e.Property(a => a.FileUrl).HasMaxLength(500).IsRequired();
                e.Property(a => a.MimeType).HasMaxLength(100).IsRequired();
                e.Property(a => a.ThumbnailUrl).HasMaxLength(500);
                e.Property(a => a.Type)
                    .HasConversion<string>()
                    .HasMaxLength(20);

                e.HasOne(a => a.Message)
                    .WithMany(m => m.Attachments)
                    .HasForeignKey(a => a.MessageId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ── Seed Data (optional) ──────────────────────────
            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            // Admin account mặc định
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 1,
                Email = "admin@nexchat.local",
                // BCrypt hash của "Admin@123" — thay bằng hash thật khi build
                PasswordHash = "$2a$11$examplehashforadmin",
                DisplayName = "NexChat Admin",
                Role = "Admin",
                IsActive = true,
                IsOnline = false,
                CreatedAt = new System.DateTime(2026, 1, 1, 0, 0, 0, System.DateTimeKind.Utc),
                UpdatedAt = new System.DateTime(2026, 1, 1, 0, 0, 0, System.DateTimeKind.Utc)
            });

            // Group chat mặc định: General
            modelBuilder.Entity<Conversation>().HasData(new Conversation
            {
                Id = 1,
                Name = "General",
                Type = ConversationType.Group,
                Description = "Kênh chung cho tất cả mọi người",
                IsActive = true,
                CreatedAt = new System.DateTime(2026, 1, 1, 0, 0, 0, System.DateTimeKind.Utc)
            });

            // Admin tham gia General
            modelBuilder.Entity<ConversationMember>().HasData(new ConversationMember
            {
                Id = 1,
                ConversationId = 1,
                UserId = 1,
                Role = "Admin",
                JoinedAt = new System.DateTime(2026, 1, 1, 0, 0, 0, System.DateTimeKind.Utc)
            });
        }
    }
}
