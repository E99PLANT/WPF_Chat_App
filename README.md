# 💬 NexChat — WPF Chat Application

<div align="center">

![WPF](https://img.shields.io/badge/Platform-WPF%20(.NET%208)-512BD4?style=for-the-badge&logo=dotnet)
![SignalR](https://img.shields.io/badge/Realtime-SignalR-FF6B35?style=for-the-badge)
![EF Core](https://img.shields.io/badge/ORM-Entity%20Framework%20Core-009688?style=for-the-badge)
![MVVM](https://img.shields.io/badge/Pattern-MVVM-3B3A8B?style=for-the-badge)

**NexChat** là ứng dụng chat nhóm thời gian thực xây dựng trên nền tảng WPF, hỗ trợ nhắn tin văn bản, gửi file, tạo nhóm và quản lý thành viên.

</div>

---

## 📸 Giao diện

```
┌─────────────────────────────────────────────────────────────┐
│  NexChat                                          [─][□][✕] │
├──────────────┬──────────────────────────────────────────────┤
│  🔍 Search   │  📌 General                    🔔  ⋯         │
│──────────────│──────────────────────────────────────────────│
│  💬 CHATS    │                                              │
│              │   NexChat Admin  10:25                       │
│  General  🔴3│   ╭──────────────────────╮                   │
│  Dev Team    │   │ Chào mọi người! 👋   │                   │
│  Alice       │   ╰──────────────────────╯                   │
│              │                                              │
│              │          ╭──────────────────╮                │
│              │          │  Xin chào Admin! │                │
│              │          ╰──────────────────╯                │
│              │                                              │
│              │   Alice đang gõ...                           │
│──────────────│──────────────────────────────────────────────│
│ [👤 User]  ➕│  😊  📎  🖼  │ Nhập tin nhắn...    [Gửi ➤]  │
└──────────────┴──────────────────────────────────────────────┘
```

---

## ✨ Tính năng chính

### 🔐 Xác thực người dùng
| Tính năng | Mô tả |
|-----------|-------|
| **Đăng ký** | Tạo tài khoản với tên, email, mật khẩu |
| **Đăng nhập** | Xác thực bằng email + mật khẩu (BCrypt) |
| **Đăng xuất** | Hỗ trợ đăng xuất và chuyển về màn hình Login |
| **Đổi mật khẩu** | Xác minh mật khẩu cũ trước khi đổi |
| **Hiện/ẩn mật khẩu** | Toggle 👁/🙈 trên các ô mật khẩu |
| **Phân quyền** | Phân biệt `Admin` và `Member` |

---

### 💬 Nhắn tin thời gian thực
| Tính năng | Mô tả |
|-----------|-------|
| **Gửi tin nhắn văn bản** | Nhắn tin tức thì qua SignalR |
| **Gửi file đính kèm** | Upload và download file bất kỳ (≤ 25 MB) |
| **Xem ảnh & Video** | Xem trực tiếp ảnh và video (.mp4) ngay trong bong bóng chat |
| **Download tệp** | Nút 📥 riêng biệt để tải mọi tệp đính kèm về máy |
| **Tin nhắn hệ thống** | Tự động thông báo khi có người `Tham gia / Bị xoá / Rời đi` |
| **Khóa chat tự động** | Thay thế khung chat bằng cảnh báo View-only khi người dùng không còn trong nhóm |
| **Emoji Picker** | Chọn và chèn emoji vào tin nhắn 😊 |
| **Enter để gửi** | Nhấn `Enter` gửi, `Shift+Enter` xuống dòng |
| **Bubble phân biệt** | Tin của mình (phải, màu tím) — Tin người khác (trái, màu trắng) |
| **Auto-scroll** | Tự cuộn xuống tin nhắn mới nhất |
| **Typing indicator** | Hiển thị "*[Tên] đang gõ...*" khi đối phương đang nhập |
| **Debounce typing** | Chỉ gửi signal "đang gõ" mỗi 1.5 giây, tránh spam |

---

### 🏘️ Quản lý nhóm (Group Chat)
| Tính năng | Mô tả |
|-----------|-------|
| **Tạo nhóm mới** | Đặt tên nhóm, creator tự động là Admin |
| **Xem danh sách thành viên** | Hiển thị tất cả members trong nhóm |
| **Thêm thành viên** | Admin thêm user qua email |
| **Xóa thành viên** | Admin kick thành viên khỏi nhóm |
| **Rời nhóm** | Member rời khỏi nhóm |
| **Giải tán nhóm** | Admin có thể xóa cả nhóm (Disband) |
| **Phân quyền Admin/Member** | Admin có quyền quản lý, Member chỉ nhắn tin |

---

### 📩 Tin nhắn trực tiếp (Direct Message)
| Tính năng | Mô tả |
|-----------|-------|
| **DM 1-1** | Tìm kiếm bằng `Email` hoặc `Tên/DisplayName` để mở chat riêng |
| **Không tự chat** | Báo lỗi hoặc chặn nếu tự nhập email của bản thân |
| **Tự động tạo DM** | Nếu chưa có DM, tự tạo conversation kiểu Direct Message |
| **Lịch sử tin nhắn** | Load 50 tin nhắn gần nhất khi mở chat (auto scroll) |

---

### 📋 Sidebar & Điều hướng
| Tính năng | Mô tả |
|-----------|-------|
| **Danh sách cuộc trò chuyện** | Hiển thị tất cả Group + DM của user |
| **Sắp xếp theo thời gian** | Conversation có tin nhắn mới nhất lên đầu |
| **Tìm kiếm real-time** | Filter tên conversation ngay khi gõ |
| **Unread badge** | Hiển thị số tin chưa đọc & Bôi đậm tên cuộc trò chuyện |
| **Preview tin cuối** | Hiển thị nội dung/tên file của tin nhắn gần nhất |
| **Active highlight** | Highlight conversation đang chọn |

---

### 🔔 Real-time SignalR
| Event | Hướng | Mô tả |
|-------|-------|-------|
| `ReceiveMessage` | Server → Client | Nhận tin nhắn mới |
| `UserTyping` | Server → Client | Ai đó đang gõ |
| `UserOnlineStatus` | Server → Client | Trạng thái online/offline |
| `SendMessage` | Client → Server | Gửi tin nhắn |
| `JoinGroup` | Client → Server | Vào kênh để nhận tin |
| `LeaveGroup` | Client → Server | Rời kênh khi chuyển tab |
| `UserTyping` (Invoke) | Client → Server | Thông báo đang gõ |
| Auto Reconnect | — | Tự kết nối lại khi mất mạng |

---

## 🏗️ Kiến trúc hệ thống

```
┌──────────────────────────────────────────────────────────┐
│                     WPF CLIENT                           │
│                                                          │
│  Views (XAML)                                            │
│    ├── LoginWindow       RegisterWindow                  │
│    ├── MainWindow        ChangePasswordWindow            │
│    ├── CreateGroupWindow GroupSettingsWindow             │
│    │                                                     │
│  SignalR Server (Embedded)                               │
│    └── Microsoft.AspNetCore.App / ChatHub                │
│    │                                                     │
│  ViewModels (CommunityToolkit.Mvvm)                      │
│    ├── MainViewModel     ConversationViewModel           │
│    └── MessageViewModel                                  │
│    │                                                     │
│  Controllers (Mediator)                                  │
│    ├── ChatController    UserController                  │
│    │                                                     │
│  Services (Business Logic)                               │
│    ├── UserService       ConversationService             │
│    ├── MessageService    SignalRService                   │
│    │                                                     │
│  Repositories (Data Access)                              │
│    ├── UserRepository    ConversationRepository          │
│    └── MessageRepository                                 │
│    │                                                     │
│  Models / Entities                                       │
│    ├── User  Conversation  ConversationMember            │
│    ├── Message  MessageRead  Attachment                  │
│    └── NexChatDbContext (EF Core Code First)             │
└──────────────────────────────────────────────────────────┘
         │ SignalR Client                   │ EF Core
│ (Internal Loopback on Port 5000)   │
▼                                   ▼
┌───────────────────────────────────────┐          ┌──────────────┐
│       EMBEDDED SIGNALR SERVER         │          │  SQL Server  │
│    (Chạy ẩn trong tiến trình WPF)     │          │  NexChatDb   │
└───────────────────────────────────────┘          └──────────────┘
```

> [!NOTE]
> NexChat sử dụng kiến trúc **Self-Hosted SignalR**. Cửa sổ ứng dụng đầu tiên được mở sẽ đóng vai trò là Hub Server. Các cửa sổ tiếp theo sẽ tự động kết nối tới Hub này như một Client thông thường.

---

## 🗄️ Cấu trúc Database

```
Users ──────────────────────────────────────┐
  id, email, passwordHash, displayName       │
  role, avatarUrl, isOnline, isActive        │
  createdAt, updatedAt                       │
                                             │
Conversations                    ConversationMembers
  id, name, type (Group/DM)    ◄──  conversationId, userId
  description, avatarUrl            role (Admin/Member)
  lastMessagePreview                unreadCount, isMuted
  lastMessageAt, isActive           hasLeft, joinedAt

Messages ────────────────────────────────────┐
  id, conversationId, senderId               │
  content, type (Text/Image/File/System)     │
  status (Sent/Delivered/Read)               │
  replyToMessageId, isDeleted, createdAt     │
                                             │
Attachments                      MessageReads
  messageId, fileName            ◄──  messageId, userId
  storedFileName (GUID)               readAt
  fileUrl, mimeType, sizeBytes
  thumbnailUrl, type (Image/File/Video)
```

---

## 📦 Cài đặt & Chạy

### Yêu cầu
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/sql-server) (LocalDB hoặc Full)
- Visual Studio 2022+
- **Lưu ý**: Không cần cài đặt Server SignalR riêng lẻ vì đã được nhúng sẵn.

### Các bước

**1. Clone project**
```bash
git clone <repo-url>
cd ASS1_B3W_SP26/Chat_Group_System
```

**2. Cấu hình connection string**

Mở `appsettings.json` và cập nhật:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=NexChatDb;Trusted_Connection=true;TrustServerCertificate=true;"
  },
  "SignalR": {
    "HubUrl": "http://localhost:5000/chatHub"
  }
}
```

**3. Chạy migration để tạo database**
```bash
dotnet ef database update
```
> Lệnh này sẽ tạo database `NexChatDb` với tài khoản Admin mặc định.

**4. Chạy ứng dụng**
```bash
dotnet run
```
Hoặc nhấn **F5** trong Visual Studio.

### Tài khoản Admin mặc định
| Field | Giá trị |
|-------|---------|
| Email | `admin@nexchat.local` |
| Password | `Admin@123` |

---

## 📚 Dependencies

| Package | Version | Mục đích |
|---------|---------|----------|
| `Microsoft.AspNetCore.SignalR.Client` | 8.x | Kết nối SignalR Hub |
| `CommunityToolkit.Mvvm` | 8.x | `[ObservableProperty]`, MVVM pattern |
| `Microsoft.Extensions.DependencyInjection` | 8.x | DI Container |
| `Microsoft.Extensions.Configuration` | 8.x | Đọc `appsettings.json` |
| `Microsoft.EntityFrameworkCore.SqlServer` | 8.x | Code First ORM |
| `Microsoft.EntityFrameworkCore.Tools` | 8.x | Migration CLI |
| `BCrypt.Net-Next` | 4.x | Hashing mật khẩu chuẩn an toàn cao |
| `System.Text.Json` | built-in | Parse JSON từ SignalR payload |

---

## 🗂️ Cấu trúc thư mục

```
Chat_Group_System/
├── App.xaml / App.xaml.cs           # Startup, DI Container
├── MainWindow.xaml / .cs             # Cửa sổ chính
├── MessageTemplateSelector.cs        # DataTemplateSelector cho bubble
├── Converters.cs                     # Value Converters (IValueConverter)
├── appsettings.json                  # Cấu hình kết nối
│
├── ViewModels/
│   ├── MainViewModel.cs              # State quản lý toàn bộ main window
│   ├── ConversationViewModel.cs      # Dữ liệu hiển thị 1 conversation
│   └── MessageViewModel.cs          # Dữ liệu hiển thị 1 tin nhắn
│
├── Controllers/
│   ├── ChatController.cs             # Mediator: Service ↔ UI
│   └── UserController.cs            # Mediator: Auth ↔ UI
│
├── Services/
│   ├── IUserService.cs / UserService.cs
│   ├── IConversationService.cs / ConversationService.cs
│   ├── IMessageService.cs / MessageService.cs
│   └── ISignalRService.cs / SignalRService.cs
│
├── Repositories/
│   ├── IUserRepository.cs / UserRepository.cs
│   ├── IConversationRepository.cs / ConversationRepository.cs
│   └── IMessageRepository.cs / MessageRepository.cs
│
├── Models/
│   ├── Data/NexChatDbContext.cs      # DbContext + EF Config + SeedData
│   └── Entities/
│       ├── User.cs
│       ├── Conversation.cs
│       ├── ConversationMember.cs
│       ├── Message.cs
│       ├── MessageRead.cs
│       └── Attachment.cs
│
├── Views/
│   ├── LoginWindow.xaml / .cs
│   ├── RegisterWindow.xaml / .cs
│   ├── ChangePasswordWindow.xaml / .cs
│   ├── CreateGroupWindow.xaml / .cs
│   └── GroupSettingsWindow.xaml / .cs
│
├── Helpers/
│   └── TimeHelper.cs                 # TimeZone helper (SE Asia Standard Time)
│
├── Hubs/
│   └── ChatHub.cs                    # Centralized SignalR Hub (Real-time Routing)
│
└── Repositories/
```

---

## 🔒 Bảo mật & Kiến trúc mở rộng

| Cơ chế | Mô tả |
|--------|-------|
| **Password Hashing** | Sử dụng thư viện `BCrypt.Net-Next` băm mật khẩu + salt an toàn |
| **Race Condition / DI Scopes** | Tách biệt các tác vụ DBContext (CreateScope) để tránh Exception đa luồng |
| **Soft Delete** | User/Message bị xóa mềm (`isDeleted`, `isActive`) |
| **Permission Check** | Chỉ Admin mới được add/remove/disband group |
| **Email Unique** | Không thể đăng ký 2 tài khoản cùng email |
| **Inactive User Block** | User bị vô hiệu hóa không thể đăng nhập |

---

<!-- ## 👥 Nhóm phát triển

| Thành viên | Vai trò |
|------------|---------|
| *(Tên thành viên 1)* | Backend, Database Design |
| *(Tên thành viên 2)* | UI/UX, XAML |
| *(Tên thành viên 3)* | SignalR, Real-time Logic |
| *(Tên thành viên 4)* | Testing, Documentation |

--- -->

## 📄 License

MIT License — Dự án học tập PRN222 B3W SP26.

---

<div align="center">

*NexChat WPF v1.1 — PRN222 Assignment · 2026*

</div>
