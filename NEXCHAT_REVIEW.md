# NexChat — Full Source Code Review

> **Ngày review:** 2026-04-18 · Reviewer: Antigravity AI  
> **Phạm vi:** Toàn bộ source code — 32 files  
> **So sánh với:** `NEXCHAT_DESIGN.md` (Design Spec v1.0)

---

## 📁 Cấu trúc files đã review

```
Chat_Group_System/
├── App.xaml / App.xaml.cs
├── MainWindow.xaml / MainWindow.xaml.cs
├── Converters.cs
├── NexChatDbContextFactory.cs
├── appsettings.json
│
├── Models/
│   ├── Data/NexChatDbContext.cs
│   └── Entities/
│       ├── User.cs
│       ├── Conversation.cs
│       ├── ConversationMember.cs
│       ├── Message.cs
│       ├── MessageRead.cs
│       └── Attachment.cs
│
├── Repositories/
│   ├── IUserRepository.cs / UserRepository.cs
│   ├── IConversationRepository.cs / ConversationRepository.cs
│   └── IMessageRepository.cs / MessageRepository.cs
│
├── Services/
│   ├── IUserService.cs / UserService.cs
│   ├── IConversationService.cs / ConversationService.cs
│   ├── IMessageService.cs / MessageService.cs
│   └── ISignalRService.cs / SignalRService.cs
│
├── Controllers/
│   ├── UserController.cs
│   └── ChatController.cs
│
└── Views/
    ├── LoginWindow.xaml / .cs
    ├── RegisterWindow.xaml / .cs
    ├── ChangePasswordWindow.xaml / .cs
    ├── CreateGroupWindow.xaml / .cs
    └── GroupSettingsWindow.xaml / .cs
```

---

## ✅ Điểm mạnh

### 1. Kiến trúc phân tầng chuẩn
- DI Container setup hoàn chỉnh trong `App.xaml.cs` với đúng lifetime:
  - `AddDbContext` — scoped (đúng)
  - `AddSingleton<ISignalRService>` — đúng vì SignalR cần 1 connection duy nhất
  - `AddScoped` cho Repository/Service/Controller — đúng
  - `AddTransient` cho Window — đúng
- `ChatController` là mediator sạch — bọc event SignalR và expose lên View, không để View touch Service trực tiếp.

### 2. Data Model thiết kế tốt
- `Message.cs`: Có `MessageStatus` (Sent/Delivered/Read), `MessageType` (Text/File/Image/System), `ReplyToMessageId` cho reply, `IsDeleted` + `DeletedAt` cho soft-delete — đây là data model cấp production.
- `ConversationMember.cs`: Có `UnreadCount`, `IsMuted`, `HasLeft`, `LeftAt` — rất đầy đủ.
- `Attachment.cs`: Có `ThumbnailUrl`, `ImageWidth/Height`, `StoredFileName` (GUID), `MimeType`.
- `NexChatDbContext.cs`: EF Core config rất chuẩn — unique index, MaxLength, HasConversion cho enum, cascade delete hợp lý, tránh cascade cycle với `DeleteBehavior.NoAction`.

### 3. UI màu sắc bám Design Spec
- Tất cả hex color trong XAML (`#3B3A8B`, `#F8F7F3`, `#22C55E`, `#F0F0F0`...) khớp chính xác với spec.
- Sidebar 195px fixed, layout 3 vùng đúng thiết kế.
- Bubble alignment dùng `IValueConverter` — MVVM-friendly, clean.

### 4. Service Layer có logic nghiệp vụ đúng
- `ConversationService.LeaveOrDisbandGroupAsync`: Admin → disband cả group, Member → chỉ rời — đúng.
- `ConversationService.AddMemberToGroupAsync`: Check quyền Admin trước — đúng.
- `MessageService.SendTextMessageAsync`: Lưu DB → Update `LastMessagePreview` → return message kèm navigation props — đúng flow.
- `MessageService.SendAttachmentMessageAsync`: Tự set đúng `MimeType` và `AttachmentType` theo `MessageType`.

### 5. UX nhỏ được chú ý
- `WithAutomaticReconnect()` trong SignalR — xử lý mất kết nối tự động.
- Auto-scroll dùng `Dispatcher.InvokeAsync` với `DispatcherPriority.Background` — chờ render xong mới scroll.
- Filter conversation dùng `ICollectionView` — cách chuẩn WPF, không lọc trực tiếp collection.
- Show/Hide password trong Login và Register bằng `ToggleButton` — UX tốt.
- Placeholder bằng `GotFocus`/`LostFocus` — thay vì hack text, hoạt động đúng.
- `db.Database.Migrate()` on startup — tự apply migration, tiện khi deploy.

---

## 🐛 Bugs & Lỗi nghiêm trọng

### BUG 1: `HashPassword` không hash — lưu plain text vào DB
**File:** `UserService.cs` — dòng 76–81  
**Mức độ:** 🔴 CRITICAL (bảo mật)

```csharp
// ❌ LỖI NGHIÊM TRỌNG — trả về plain text, không hash gì cả
private string HashPassword(string plainPassword)
{
    return plainPassword;  // <-- mật khẩu được lưu thẳng vào DB không mã hóa!
}
```

**Fix:** Cần cài BCrypt.Net hoặc dùng `SHA256`:
```csharp
// Cách 1: Dùng BCrypt (khuyên dùng) — install BCrypt.Net-Next
private string HashPassword(string plainPassword)
{
    return BCrypt.Net.BCrypt.HashPassword(plainPassword);
}

// Cách 2: Dùng SHA256 (nếu không muốn thêm package)
private string HashPassword(string plainPassword)
{
    using var sha256 = System.Security.Cryptography.SHA256.Create();
    var bytes = System.Text.Encoding.UTF8.GetBytes(plainPassword);
    return Convert.ToBase64String(sha256.ComputeHash(bytes));
}
```

---

### BUG 2: SeedData dùng BCrypt hash giả — login Admin sẽ fail
**File:** `NexChatDbContext.cs` — dòng 151

```csharp
// ❌ Hash giả — không match với bất kỳ password nào
PasswordHash = "$2a$11$examplehashforadmin",
```

Vì `HashPassword` trả về plain text (BUG 1), seed data cần đồng bộ:
```csharp
// ✅ Đồng bộ với HashPassword hiện tại (plain text)
PasswordHash = "Admin@123",

// ✅ Hoặc sau khi fix BCrypt:
PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
```

---

### BUG 3: `LoginWindow` không dùng constructor injection
**File:** `LoginWindow.xaml.cs` — dòng 13–17

```csharp
// ❌ SAI — constructor không nhận UserController từ DI
public LoginWindow()
{
    InitializeComponent();
    _userController = App.ServiceProvider.GetRequiredService<UserController>(); // Service Locator anti-pattern
}
```

Tất cả View khác (`GroupSettingsWindow`) đã làm đúng:
```csharp
// ✅ ĐÚNG — nhận từ DI qua constructor
public GroupSettingsWindow(ChatController chatController, UserController userController)
{
    _chatController = chatController;
    _userController = userController;
}
```

`LoginWindow` và `RegisterWindow` nên làm tương tự. Hiện tại dùng Service Locator trong constructor — không nhất quán.

---

### BUG 4: `MainWindow.ConvList` không sync với `SelectedConversation`
**File:** `MainWindow.xaml.cs` — dòng 114–122

```csharp
// ❌ Set SelectedConversation trực tiếp mà không set ConvList.SelectedItem
SelectedConversation = Conversations[0];
var messages = await _chatController.GetRecentMessagesAsync(SelectedConversation.Id);
```

**Hậu quả:** Khi app load, `Conversations[0]` được hiển thị trong header và load tin nhắn, nhưng ListBox không highlight item đầu tiên được chọn — UI không đồng bộ.

**Fix:**
```csharp
// ✅ Set qua ListBox để trigger SelectionChanged đúng cách, hoặc:
ConvList.SelectedIndex = 0;
// Hoặc set qua binding:
SelectedConversation = Conversations[0]; // và đảm bảo binding TwoWay đã set ConvList.SelectedItem
```

---

### BUG 5: `MessageRepository.GetMessagesByConversationIdAsync` trả về tin theo thứ tự ngược
**File:** `MessageRepository.cs` — dòng 27

```csharp
// ❌ OrderByDescending → tin mới nhất ở đầu, cũ ở cuối
.OrderByDescending(m => m.CreatedAt)
.Skip(skip)
.Take(take)
```

**Hậu quả:** Khi load 50 tin nhắn gần nhất, chúng được hiển thị từ mới → cũ thay vì cũ → mới. Tin nhắn đầu tiên trong danh sách là tin mới nhất, không phải cũ nhất.

**Fix:**
```csharp
// ✅ Đúng: lấy 50 tin mới nhất nhưng hiển thị theo thứ tự tăng dần
.OrderByDescending(m => m.CreatedAt)
.Skip(skip)
.Take(take)
.OrderBy(m => m.CreatedAt)  // ← đảo lại để render đúng thứ tự
.ToListAsync();
```

---

### BUG 6: `ConversationRepository.AddAsync` — Creator không được set role Admin
**File:** `ConversationRepository.cs` — dòng 52–58

```csharp
// ❌ Tất cả participant (kể cả creator) đều được add với role "Member"
var members = participantIds.Select(id => new ConversationMember
{
    Role = "Member" // ← Creator phải là Admin!
});
```

**Hậu quả:** Người tạo group không có quyền Admin → không thể add/remove thành viên, không thể disband group.

**Fix trong** `ConversationService.CreateGroupChatAsync`:
```csharp
// ✅ Truyền creatorId riêng để Repo phân biệt
await _conversationRepository.AddAsync(conversation, allMembers, creatorId);

// Trong Repo:
var members = participantIds.Select(id => new ConversationMember
{
    ConversationId = conversation.Id,
    UserId = id,
    Role = id == creatorId ? "Admin" : "Member"  // ← fix ở đây
});
```

---

### BUG 7: `BtnNewChat_Click` tạo `CreateGroupWindow` không qua DI
**File:** `MainWindow.xaml.cs` — dòng 297

```csharp
// ❌ Bypass DI — CreateGroupWindow cũng chưa được đăng ký vào container
var createGroupWin = new Views.CreateGroupWindow();
```

**Fix:**
```csharp
// ✅ Dùng ServiceProvider
var createGroupWin = App.ServiceProvider.GetRequiredService<Views.CreateGroupWindow>();
```
Và thêm vào `ConfigureServices`:
```csharp
services.AddTransient<Views.CreateGroupWindow>();
```

---

### BUG 8: `appsettings.json` chứa SignalR URL nhưng `SignalRService` hardcode URL khác
**File:** `SignalRService.cs` — dòng 18 vs `appsettings.json` — dòng 6

```csharp
// ❌ SignalRService hardcode URL riêng, bỏ qua appsettings.json
var hubUrl = $"https://localhost:5001/chatHub?userId={userId}";
```

```json
// appsettings.json có URL khác (http, port 5000)
"HubUrl": "http://localhost:5000/chatHub"
```

**Fix:** Inject `IConfiguration` vào `SignalRService` và đọc từ config:
```csharp
public SignalRService(IConfiguration config)
{
    _hubUrl = config["SignalR:HubUrl"]!;
}
```

---

### BUG 9: `MainWindow` vi phạm MVVM — View là ViewModel
**File:** `MainWindow.xaml.cs`

```csharp
// ❌ Code-behind implement INotifyPropertyChanged và chứa toàn bộ business state
public partial class MainWindow : Window, INotifyPropertyChanged
{
    public ObservableCollection<Conversation> Conversations { get; set; } = new();
    public ObservableCollection<Message> CurrentMessages { get; set; } = new();
    public Conversation? SelectedConversation { ... }
    ...
}
```

Design Spec đã vạch ra `ViewModels/` nhưng chưa được tạo. Cần tách:
```
ViewModels/
├── MainViewModel.cs          ← Conversations, SelectedConversation, NewChatCommand
├── ConversationViewModel.cs  ← Name, LastMessage, UnreadCount, IsOnline
└── MessageViewModel.cs       ← Content, IsSelf, Status, Type, FileName
```

---

## ⚠️ Vấn đề thiếu tính năng (so với Design Spec)

| Tính năng | File liên quan | Trạng thái |
|---|---|---|
| Online dot (🟢/⚫) trên avatar | `MainWindow.xaml` | ❌ Chưa có |
| Unread badge đỏ | `MainWindow.xaml` | ❌ Chưa có |
| Last message preview trong sidebar | `MainWindow.xaml` | ❌ Hiện `Type` thay vì preview |
| Typing indicator (3 chấm bounce) | `MainWindow.xaml.cs` L158 | ⚠️ Handler trống |
| Online status dot update | `MainWindow.xaml.cs` L168 | ⚠️ Handler trống |
| `JoinGroup` SignalR invoke | `SignalRService.cs` | ❌ Method không tồn tại |
| Read receipt hiển thị (✓/✓✓/xanh) | `MainWindow.xaml` | ❌ Chưa có |
| Animation bubble slide-in | `MainWindow.xaml` | ❌ Chưa có |
| Emoji Picker popup | `MainWindow.xaml` | ❌ Button có, không có handler |
| Resource Dictionary (Colors/Typography) | `Themes/` | ❌ Tất cả hardcode trong XAML |
| Separate Self/Other bubble DataTemplates | Converters.cs | ⚠️ Dùng Converter thay vì DataTemplateSelector |
| Bubble CornerRadius Self khác Other | `MainWindow.xaml` L190 | ⚠️ Dùng chung `16,16,16,4` |
| `NotifyTyping` khi user gõ | `MainWindow.xaml.cs` | ❌ Chưa gọi `NotifyTypingAsync` |
| Debounce timer cho typing | `MainWindow.xaml.cs` | ❌ Chưa có |

---

## ⚠️ Code Smell & Cải thiện

### SMELL 1: Hardcode `DateTime.UtcNow.AddHours(7)` khắp nơi
**Files:** Tất cả Entity, Repository, Service — xuất hiện **12+ lần**

```csharp
// ❌ Magic number, không nhất quán, sai khi deploy ở timezone khác
CreatedAt = DateTime.UtcNow.AddHours(7)
```

**Fix:** Dùng `DateTimeOffset.UtcNow` hoặc tạo helper:
```csharp
// Trong Entity base class hoặc static helper
public static DateTime NowVN => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, 
    TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
```

---

### SMELL 2: `ConversationRepository.AddAsync` — `SaveChanges` gọi 2 lần không cần thiết
**File:** `ConversationRepository.cs` — dòng 49–63

```csharp
await _context.Conversations.AddAsync(conversation);
await _context.SaveChangesAsync();  // ← lần 1

await _context.ConversationMembers.AddRangeAsync(members);
await _context.SaveChangesAsync();  // ← lần 2
```

**Hậu quả:** Nếu lần 2 fail thì Conversation đã được tạo nhưng không có member — data inconsistent.

**Fix:** Gộp thành 1 transaction:
```csharp
await _context.Conversations.AddAsync(conversation);
await _context.SaveChangesAsync();  // cần để có conversation.Id

// Sau đó add members và save 1 lần nữa — OK về mặt transaction trong EF Core scope
// Hoặc dùng explicit transaction:
using var tx = await _context.Database.BeginTransactionAsync();
// ... add conversation, save, add members, save, commit
await tx.CommitAsync();
```

---

### SMELL 3: `Attachment.StoredFileName` luôn bằng `FileName` — ý nghĩa bị mất
**File:** `MessageService.cs` — dòng 56–57

```csharp
FileName = fileName,
StoredFileName = fileName,  // ← nên là GUID-based unique name
```

Theo design của `Attachment.cs` là: `StoredFileName` = GUID để tránh trùng tên file trên server.

**Fix:**
```csharp
StoredFileName = $"{Guid.NewGuid()}_{fileName}",
```

---

### SMELL 4: `GetDirectMessageAsync` có thể trả về kết quả sai với group nhiều người
**File:** `ConversationRepository.cs` — dòng 38–45

```csharp
// ⚠️ Logic này chỉ check "có cả 2 user không" nhưng không check "chỉ có đúng 2 user"
// → Có thể trả về 1 GroupChat có cả 2 user thay vì DM
return await _context.Conversations
    .Where(c => c.Type == ConversationType.DirectMessage)  // ← filter Type đã giải quyết vấn đề
    .FirstOrDefaultAsync(c => 
        c.Members.Any(m => m.UserId == userId1) && 
        c.Members.Any(m => m.UserId == userId2));
```

Thực ra đã filter `ConversationType.DirectMessage` nên OK. Nhưng cần thêm check số lượng member = 2 để chắc chắn:
```csharp
.Where(c => c.Type == ConversationType.DirectMessage && c.Members.Count == 2)
```

---

### SMELL 5: `LoginWindow` constructor không nhất quán với các View khác
**File:** `LoginWindow.xaml.cs`, `RegisterWindow.xaml.cs`

Cả 2 dùng parameterless constructor và lấy service từ `App.ServiceProvider` trực tiếp. Trong khi `GroupSettingsWindow` và `MainWindow` dùng constructor injection đúng chuẩn. Cần thống nhất.

---

### SMELL 6: `appsettings.json` có `JwtSecret` nhưng không có JWT implementation nào
**File:** `appsettings.json` — dòng 9

```json
"JwtSecret": "NexChat@SuperSecretKey2026!ChangeInProd"
```

Key này chưa được dùng ở đâu — chỉ là placeholder. Nên xóa để tránh nhầm lẫn hoặc implement nếu cần.

---

## 📋 Checklist Design Spec — Tiến độ chính xác

### Phase 1 — Foundation
- [x] Setup WPF project + DI
- [x] Layout 3 vùng Grid
- [ ] Resource Dictionaries (Colors.xaml, Typography.xaml, Animations.xaml) — hardcode trong XAML

### Phase 2 — Sidebar
- [x] ListBox với ItemContainerStyle active highlight
- [x] Section label (CHATS)
- [x] Search box với ICollectionView filter
- [ ] Avatar với Online Dot (🟢/⚫)
- [ ] Unread badge đỏ
- [ ] LastMessagePreview (chỉ hiện Type)

### Phase 3 — Chat Area
- [x] ChatHeader — tên, action icons
- [x] MessageBubble cơ bản (Text + File)
- [ ] TypingIndicator UserControl (3 chấm bounce)
- [x] Auto-scroll khi có tin mới
- [ ] Bubble CornerRadius đúng Self/Other
- [ ] Sender name hiển thị trong bubble (Group chat)

### Phase 4 — Input Area
- [x] TextBox với placeholder UX
- [x] Toolbar icons (File, Image)
- [ ] Emoji Picker popup (button có, handler không)
- [x] Send on Enter, Shift+Enter = newline

### Phase 5 — SignalR Integration
- [x] SignalRService — connect/disconnect
- [x] Subscribe: ReceiveMessage, UserTyping, UserOnlineStatus
- [x] Invoke: SendMessage
- [ ] Invoke: JoinGroup khi chọn conversation
- [ ] NotifyTyping khi user gõ
- [ ] Typing debounce timer (500ms)

### Phase 6 — Polish
- [ ] Bubble slide-in animation
- [ ] Read receipt (✓ → ✓✓ → xanh)
- [ ] Notification sound
- [ ] Window title badge [3] NexChat

---

## 🚀 Ưu tiên fix theo thứ tự

| # | Mức | Việc cần làm | Lý do |
|---|---|---|---|
| 1 | 🔴 BUG | Fix `HashPassword` — implement SHA256 hoặc BCrypt | Bảo mật |
| 2 | 🔴 BUG | Fix SeedData — đồng bộ `PasswordHash` với cách hash | Login Admin fail |
| 3 | 🔴 BUG | Fix `ConversationRepository.AddAsync` — Creator = Admin | Group owner không có quyền |
| 4 | 🔴 BUG | Fix `MessageRepository` — `OrderBy` sau `Take` | Message hiện ngược thứ tự |
| 5 | 🔴 BUG | Fix `SignalRService` — đọc HubUrl từ `appsettings.json` | Hardcode port sai |
| 6 | 🟡 BUG | Fix `BtnNewChat_Click` — dùng DI | Inconsistent |
| 7 | 🟡 BUG | Fix `MainWindow.Loaded` — sync `ConvList.SelectedIndex` | UI không highlight item đầu |
| 8 | 🟡 CODE | Thống nhất constructor injection cho LoginWindow/RegisterWindow | Design consistency |
| 9 | 🟡 CODE | `StoredFileName` = GUID-based | Tránh trùng tên file |
| 10 | 🟡 CODE | Xóa/replace magic number `AddHours(7)` | Maintainability |
| 11 | 🟢 FEAT | Implement Typing Indicator UI (animation 3 chấm) | UX feature |
| 12 | 🟢 FEAT | Thêm Online Dot + Unread Badge vào sidebar | UX feature |
| 13 | 🟢 FEAT | JoinGroup invoke + NotifyTyping | SignalR feature |
| 14 | 🟢 FEAT | Tách ViewModel (MainViewModel, ConversationViewModel) | MVVM pattern |
| 15 | 🟢 FEAT | Resource Dictionary Colors.xaml / Typography.xaml | Maintainability |

---

## 📊 Điểm tổng kết

| Hạng mục | Điểm | Nhận xét |
|---|---|---|
| **Data Model** | 9/10 | Thiết kế DB rất chuyên nghiệp, EF config chuẩn |
| **Kiến trúc DI** | 8/10 | Phân tầng tốt — LoginWindow/RegisterWindow chưa dùng constructor injection |
| **Service/Repo Logic** | 7/10 | Logic đúng nhưng có bug creator role + message order |
| **Bảo mật** | 2/10 | `HashPassword` không hash — **lỗi nghiêm trọng nhất** |
| **UI/UX** | 6/10 | Màu đúng spec, thiếu online dot / badge / animation |
| **SignalR** | 6/10 | Kết nối OK, thiếu JoinGroup + NotifyTyping + hardcode URL |
| **MVVM** | 4/10 | Code-behind vừa là View vừa là ViewModel |
| **Hoàn thiện theo Spec** | 5.5/10 | ~55% feature done |
| **Tổng** | **5.9/10** | Nền tốt, một số bug nghiêm trọng cần fix ngay |

---

*Review generated: 2026-04-18 · NexChat WPF v1.0 — Full Review*
