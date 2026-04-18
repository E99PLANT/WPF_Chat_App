# NexChat — WPF Design Specification

> **Tone:** Clean & professional · Tông tối nhẹ kiểu Notion/Linear · Không quá game-y, không corporate chán.

---

## 📐 Layout Architecture

### Bố cục 3 vùng chính

```
┌─────────────────────────────────────────────────────────────────────┐
│  [Sidebar 195px]  │          [Chat Area — flexible]                 │
│                   │  ┌─────────────────────────────────────────┐   │
│  ● Group Chat     │  │  Header: Group Name · 12 online  🟢      │   │
│  ─────────────    │  ├─────────────────────────────────────────┤   │
│  ● DM             │  │                                         │   │
│                   │  │        [Message Bubble Area]            │   │
│                   │  │                                         │   │
│                   │  ├─────────────────────────────────────────┤   │
│                   │  │  [Input Area + Toolbar]                 │   │
└───────────────────┴──┴─────────────────────────────────────────┴───┘
```

---

## 🎨 Color Palette

| Token | Hex | Dùng cho |
|---|---|---|
| `--accent-primary` | `#3B3A8B` | Bubble user, active state, Send button |
| `--accent-hover` | `#4F4EAA` | Hover state của accent |
| `--accent-light` | `#EEEDF8` | Bubble background nhạt (hover row) |
| `--sidebar-bg` | `#F8F7F3` | Nền sidebar — warm off-white, giảm mỏi mắt |
| `--sidebar-active` | `#3B3A8B` | Active conversation item |
| `--sidebar-active-text` | `#FFFFFF` | Text khi active |
| `--sidebar-text` | `#3D3D3D` | Text thường trong sidebar |
| `--sidebar-muted` | `#9A9A9A` | Timestamp, sub-label |
| `--chat-bg` | `#FFFFFF` | Nền khu vực chat chính |
| `--chat-header-bg` | `#FAFAFA` | Header chat |
| `--bubble-self` | `#3B3A8B` | Bubble người dùng |
| `--bubble-self-text` | `#FFFFFF` | Text trong bubble user |
| `--bubble-other` | `#F0F0F0` | Bubble đối phương |
| `--bubble-other-text` | `#1A1A1A` | Text bubble đối phương |
| `--online-dot` | `#22C55E` | Dot trạng thái online |
| `--unread-badge` | `#EF4444` | Badge số tin chưa đọc |
| `--input-bg` | `#F5F4F0` | Input area background |
| `--border-light` | `#E8E7E3` | Divider, border mỏng |
| `--text-primary` | `#1A1A1A` | Text chính |
| `--text-secondary` | `#6B6B6B` | Text phụ, timestamp |

---

## 🖼 Typography

| Role | Font | Weight | Size |
|---|---|---|---|
| App Title / Logo | **Inter** | 700 Bold | 18px |
| Section Label | **Inter** | 600 SemiBold | 10px (UPPERCASE) |
| Conversation Name | **Inter** | 500 Medium | 13px |
| Message Preview | **Inter** | 400 Regular | 12px |
| Message Body | **Inter** | 400 Regular | 14px |
| Timestamp | **Inter** | 400 Regular | 11px |
| Send Button | **Inter** | 600 SemiBold | 13px |

> **WPF Font Binding:** Sử dụng `FontFamily="pack://application:,,,/Fonts/#Inter"` hoặc load từ Google Fonts embed.

---

## 📂 Sidebar (195px fixed width)

### Cấu trúc

```
┌─────────────────────────┐
│  🔷 NexChat        [+]  │  ← Logo + New Chat button
│  ─────────────────────  │
│  [🔍 Search...]         │  ← Search box
│                         │
│  GROUP CHATS            │  ← Section label (uppercase, muted)
│  ┌────────────────────┐ │
│  │ 🟢 [Avatar] PRN222 │ │  ← Active item (nền #3B3A8B, text trắng)
│  │         2 min ago  │ │
│  └────────────────────┘ │
│  [Avatar] DevTeam    3  │  ← Unread badge đỏ
│  [Avatar] ASM1 Group    │
│                         │
│  DIRECT MESSAGES        │  ← Section label
│  🟢 [Av] Nguyen Van A   │
│  ⚫ [Av] Tran Thi B     │
└─────────────────────────┘
```

### Conversation Item

```
┌─────────────────────────────────────┐
│ [Avatar 36px]  [Name 13px Bold]     │
│  🟢 dot        [Preview 12px muted] │  ← last message truncate
│                [Timestamp] [Badge]  │
└─────────────────────────────────────┘
```

**States:**
- **Default:** Nền `--sidebar-bg`, hover nhẹ `rgba(0,0,0,0.04)`
- **Active:** Nền `--accent-primary` (#3B3A8B), text trắng, border-radius 8px với margin 6px ngang
- **Unread:** Name bold hơn (700), badge đỏ góc phải hiện số tin

**Online Dot:**
- Kích thước: 9px × 9px, viền 2px màu sidebar-bg
- Vị trí: Bottom-right của avatar
- Màu online: `--online-dot` (#22C55E)
- Màu offline: `#C4C4C4`

---

## 💬 Chat Header

```
┌───────────────────────────────────────────────────────────┐
│  [Avatar 40px]  PRN222 — Fall Semester          [⋯] [🔔]  │
│                 🟢 12 online · 24 members                  │
└───────────────────────────────────────────────────────────┘
```

- **Border-bottom:** 1px `--border-light`
- **Height:** 64px
- **Background:** `--chat-header-bg`
- Số online hiện như `🟢 12 online` — dot xanh + text muted

---

## 🗨 Message Bubble Area

### Layout nguyên tắc

| Bubble | Alignment | Màu nền | Text |
|--------|-----------|---------|------|
| Của mình (Self) | Phải (Right) | `#3B3A8B` | `#FFFFFF` |
| Của người khác (Other) | Trái (Left) | `#F0F0F0` | `#1A1A1A` |

### Cấu trúc Bubble

**Bubble của đối phương (Left-aligned):**
```
[Avatar 32px]  ┌──────────────────────────┐
               │ Nguyen Van A             │  ← Tên người gửi (group)
               │ Xin chào mọi người! 👋   │  ← Nội dung
               └──────────────────────────┘
               10:30 AM
```

**Bubble của mình (Right-aligned):**
```
                 ┌──────────────────────────┐
                 │ Chào bạn! Có gì không?   │  ← Nền #3B3A8B
                 └──────────────────────────┘
                              10:31 AM  ✓✓  ← Timestamp + Read receipt
```

### Bubble Specs

```
Border-radius:
  - Self:  16px 16px 4px 16px  (góc phải dưới nhọn hơn)
  - Other: 16px 16px 16px 4px  (góc trái dưới nhọn hơn)

Padding:    10px 14px
Max-width:  65% của chat area
Margin:     4px 0 (cùng người), 12px 0 (người khác)
```

### Read Receipt (✓✓)

```
✓   → Đã gửi (sent)
✓✓  → Đã nhận (delivered)  — màu muted #9A9A9A
✓✓  → Đã đọc (read)        — màu xanh #3B82F6 (blue)
```

### File Attachment Bubble

```
┌──────────────────────────────────────┐
│  📄  Report_Q1.pdf                   │
│       2.4 MB  ·  Click to download  │
└──────────────────────────────────────┘
```

- Icon file theo extension: 📄 PDF, 🖼 IMG, 📊 XLSX, 📝 DOCX
- Background: `rgba(255,255,255,0.15)` nếu trong bubble self, `#E8E8E8` nếu other
- Border-radius: 8px
- Hover: cursor pointer, nhẹ underline tên file

### Image Attachment Bubble

```
┌──────────────────────────┐
│  [Image Preview 200px]   │
│  screenshot.png · 1.2MB  │
└──────────────────────────┘
```

- Preview tối đa 200px height, click để xem full
- Bo góc cùng với bubble

---

## ⌨️ Typing Indicator

```
[Avatar]  ┌─────────────────────┐
          │  ●  ●  ●            │  ← 3 chấm bouncing animation
          └─────────────────────┘
          Nguyen Van A đang nhập...
```

- Animation: CSS keyframe bounce, stagger 0.2s mỗi chấm
- Màu chấm: `#9A9A9A`
- Bubble bg: `#F0F0F0` (same as other bubble)
- Hiện dưới cùng message list

---

## 📝 Input Area

```
┌──────────────────────────────────────────────────────────────────┐
│  [😊] [📎] [🖼]  |  Nhập tin nhắn...                [➤ Gửi]     │
└──────────────────────────────────────────────────────────────────┘
```

### Specs

- **Height:** Min 56px, auto-expand tối đa 120px (multi-line)
- **Background:** `--input-bg` (#F5F4F0)
- **Border-top:** 1px `--border-light`
- **Toolbar icons:** 20px, màu `#6B6B6B`, hover `--accent-primary`

### Toolbar Actions

| Icon | Hành động |
|------|-----------|
| 😊 | Mở Emoji Picker (popup overlay) |
| 📎 | Đính kèm file (OpenFileDialog) |
| 🖼 | Đính kèm ảnh (ImageFileDialog) |
| **B** | Bold text |
| *I* | Italic text |
| ~~S~~ | Strikethrough |

### Send Button

- Màu: `--accent-primary` (#3B3A8B)
- Icon: Arrow right filled ➤
- Border-radius: 8px, padding 10px 16px
- Hover: `--accent-hover` (#4F4EAA) + nhẹ scale(1.02)
- Disabled (input rỗng): opacity 0.4, cursor not-allowed

---

## ⚡ SignalR Events & Real-time UX

### Event Map

| Event | Hướng | UI Action |
|-------|-------|-----------|
| `ReceiveMessage` | Server → Client | Append bubble mới, scroll to bottom, play sound |
| `UserTyping` | Server → Client | Hiện typing indicator 3 chấm |
| `MessageRead` | Server → Client | Update read receipt ✓✓ → xanh |
| `UserOnlineStatus` | Server → Client | Update dot xanh/xám trong sidebar + header |
| `SendMessage` | Client → Server | Gửi khi click Send hoặc Enter |
| `JoinGroup` | Client → Server | Invoke khi chọn conversation trong sidebar |

### Typing Indicator Logic

```csharp
// Client side: Debounce gửi event mỗi 500ms khi user đang gõ
private DispatcherTimer _typingTimer;

private void OnInputTextChanged(object sender, TextChangedEventArgs e)
{
    await _hubConnection.InvokeAsync("UserTyping", groupId, currentUserId);
    _typingTimer.Stop();
    _typingTimer.Start(); // reset 2s timer để hide indicator
}
```

### Online Status Dot (UserOnlineStatus)

```csharp
// Khi nhận event:
hub.On<string, bool>("UserOnlineStatus", (userId, isOnline) => {
    var item = ConversationList.FirstOrDefault(c => c.UserId == userId);
    if (item != null) item.IsOnline = isOnline;
    // Binding tự update dot color
});
```

---

## 🏗 WPF Component Structure

```
MainWindow.xaml
├── SidebarView.xaml          # UserControl — 195px sidebar
│   ├── SearchBox
│   ├── SectionLabel ("GROUP CHATS")
│   ├── ConversationListView  # ItemsControl / ListBox
│   │   └── ConversationItem.xaml
│   ├── SectionLabel ("DIRECT MESSAGES")
│   └── ConversationListView  # DM list
│
├── ChatAreaView.xaml         # UserControl — chat main
│   ├── ChatHeaderView.xaml
│   ├── MessageListView.xaml  # ScrollViewer + ItemsControl
│   │   ├── MessageBubble.xaml (Self / Other via DataTemplate)
│   │   ├── FileBubble.xaml
│   │   └── TypingIndicator.xaml
│   └── InputAreaView.xaml
│       ├── ToolbarView.xaml
│       ├── RichTextBox (input)
│       └── SendButton
│
└── EmojiPickerPopup.xaml     # Popup overlay
```

### DataTemplates Strategy

```xml
<!-- MessageTemplateSelector.cs -->
<DataTemplate x:Key="SelfMessageTemplate">
    <!-- Bubble phải, nền #3B3A8B -->
</DataTemplate>
<DataTemplate x:Key="OtherMessageTemplate">
    <!-- Bubble trái, nền #F0F0F0 -->
</DataTemplate>
<DataTemplate x:Key="FileMessageTemplate">
    <!-- File preview card -->
</DataTemplate>
```

---

## 📊 ViewModel Structure (MVVM)

```
ViewModels/
├── MainViewModel.cs
│   ├── ObservableCollection<ConversationViewModel> Conversations
│   ├── ConversationViewModel SelectedConversation
│   └── ICommand NewChatCommand
│
├── ConversationViewModel.cs
│   ├── string Name
│   ├── string LastMessage
│   ├── int UnreadCount
│   ├── bool IsOnline
│   ├── bool IsGroup
│   ├── ObservableCollection<MessageViewModel> Messages
│   └── string TypingUserName   ← binding cho typing indicator
│
└── MessageViewModel.cs
    ├── string Content
    ├── bool IsSelf
    ├── DateTime Timestamp
    ├── ReadStatus Status       ← Sent / Delivered / Read
    ├── MessageType Type        ← Text / File / Image
    ├── string? FileName
    ├── long? FileSize
    └── string? ImagePath
```

---

## 🔄 Flow Diagrams

### Send Message Flow

```
[User types] → [Enter / Click Send]
    → InvokeAsync("SendMessage", groupId, content)
    → UI: Add bubble (optimistic, status=Sent ✓)
    → Server broadcasts ReceiveMessage to group
    → Other clients: append bubble
    → Server fires MessageRead when recipient opens
    → UI: Update ✓✓ blue
```

### Join Group Flow

```
[User clicks conversation in sidebar]
    → JoinGroup event → Server.JoinGroup(groupId)
    → Load message history (REST API GET /api/messages/{groupId})
    → Populate MessageList, scroll to bottom
    → Subscribe typing / online events for this group
```

### Online Status Flow

```
[User connects] → UserOnlineStatus(userId, true) broadcast
[User disconnects] → UserOnlineStatus(userId, false) broadcast
[Sidebar] → Converter: bool IsOnline → dot color (#22C55E / #C4C4C4)
```

---

## 🎞 Animations & Micro-interactions

| Element | Animation | Duration |
|---------|-----------|----------|
| New message bubble | SlideInFromBottom + FadeIn | 200ms ease-out |
| Conversation switch | Cross-fade message list | 150ms |
| Typing indicator dots | Bounce up/down stagger | 0.6s loop |
| Send button hover | Scale 1.0 → 1.03 | 100ms |
| Unread badge | Scale pulse on new message | 300ms |
| Online dot | Fade in/out on status change | 400ms |
| Emoji picker | SlideUp + FadeIn | 180ms |

```xml
<!-- WPF Storyboard example: Bubble slide-in -->
<Storyboard x:Key="BubbleAppearStoryboard">
    <DoubleAnimation Storyboard.TargetProperty="Opacity"
                     From="0" To="1" Duration="0:0:0.2"/>
    <DoubleAnimation Storyboard.TargetProperty="(TranslateTransform.Y)"
                     From="12" To="0" Duration="0:0:0.2">
        <DoubleAnimation.EasingFunction>
            <CubicEase EasingMode="EaseOut"/>
        </DoubleAnimation.EasingFunction>
    </DoubleAnimation>
</Storyboard>
```

---

## 🗂 Resource Dictionary Structure

```
Themes/
├── Colors.xaml       # Tất cả color tokens
├── Typography.xaml   # TextBlock styles
├── Buttons.xaml      # Button styles (Send, Icon buttons)
├── Inputs.xaml       # TextBox, RichTextBox styles
├── Animations.xaml   # Storyboard definitions
└── MergedDictionary.xaml  # Entry point merge tất cả
```

```xml
<!-- Colors.xaml snippet -->
<SolidColorBrush x:Key="AccentPrimary" Color="#3B3A8B"/>
<SolidColorBrush x:Key="SidebarBg"    Color="#F8F7F3"/>
<SolidColorBrush x:Key="OnlineDot"    Color="#22C55E"/>
<SolidColorBrush x:Key="UnreadBadge"  Color="#EF4444"/>
<SolidColorBrush x:Key="BubbleSelf"   Color="#3B3A8B"/>
<SolidColorBrush x:Key="BubbleOther"  Color="#F0F0F0"/>
```

---

## 📋 Implementation Checklist *(Original)*

### Phase 1 — Foundation
- [ ] Setup WPF project + MVVM framework (CommunityToolkit.Mvvm)
- [ ] Tạo Resource Dictionaries (Colors, Typography, Animations)
- [ ] Layout 3 vùng (`Grid` với `ColumnDefinition`)

### Phase 2 — Sidebar
- [ ] `ConversationItem` UserControl với avatar, online dot, name, preview, badge
- [ ] `ListBox` với custom `ItemContainerStyle` (active highlight)
- [ ] Section labels (GROUP CHATS / DIRECT MESSAGES)
- [ ] Search box với filter binding

### Phase 3 — Chat Area
- [ ] `ChatHeaderView` — tên, online count, action icons
- [ ] `MessageBubble` — Self / Other DataTemplates
- [ ] `FileBubble` — file preview card
- [ ] `TypingIndicator` UserControl — 3 dots bounce animation
- [ ] Auto-scroll đến bottom khi có tin mới

### Phase 4 — Input Area
- [ ] `RichTextBox` với placeholder text behavior
- [ ] Toolbar icons (Emoji, File, Image, Bold, Italic)
- [ ] `EmojiPicker` Popup
- [ ] Send on Enter (Shift+Enter = newline)

### Phase 5 — SignalR Integration
- [ ] `ChatHubService` — quản lý connection lifecycle
- [ ] Subscribe: `ReceiveMessage`, `UserTyping`, `MessageRead`, `UserOnlineStatus`
- [ ] Invoke: `SendMessage`, `JoinGroup`
- [ ] Typing debounce timer

### Phase 6 — Polish
- [ ] Slide-in animation cho bubble mới
- [ ] Read receipt update (✓ → ✓✓ → xanh)
- [ ] Notification sound khi có tin ngoài focus
- [ ] Window title badge `[3] NexChat` khi có unread

---

## 📊 Implementation Status *(cập nhật 2026-04-18)*

### Phase 1 — Foundation
- [x] Setup WPF project + DI Container (`App.xaml.cs`)
- [x] Layout 3 vùng `Grid` với `ColumnDefinition` (195px + `*`)
- [x] `ViewModels/` tách biệt dùng `CommunityToolkit.Mvvm` `[ObservableProperty]`
- [ ] Resource Dictionaries (`Colors.xaml`, `Typography.xaml`, `Animations.xaml`) — **chưa tạo, màu vẫn hardcode trong XAML**

### Phase 2 — Sidebar
- [x] `ListBox` với `ItemContainerStyle` active highlight (`#3B3A8B`)
- [x] Section label (CHATS)
- [x] Search box với `ICollectionView` filter + `OnSearchTextChanged`
- [x] `ConversationViewModel.UnreadCount` — tăng badge khi nhận tin từ group nền
- [x] `ConversationViewModel.LastMessagePreview` — cập nhật khi nhận tin mới
- [ ] Online dot (🟢/⚫) trên avatar — **chưa có UI**
- [ ] Unread badge đỏ hiển thị con số trên item — **chưa có UI**

### Phase 3 — Chat Area
- [x] ChatHeader — tên conversation, action icons (🔔, ⋯)
- [x] `MessageBubble` cơ bản — Text + File attachment
- [x] `MessageTemplateSelector.cs` — `DataTemplateSelector` cho Self / Other ⚠️ *(xem Known Issues #2)*
- [x] Auto-scroll khi có tin mới (`Dispatcher.InvokeAsync`)
- [x] Typing indicator **state** (`IsTyping`, `TypingMessage` binding hoạt động)
- [ ] Typing indicator **UI** (3 chấm bounce animation XAML) — **chưa có**
- [ ] Bubble `CornerRadius` đúng Self (`16,16,4,16`) / Other (`16,16,16,4`) — **chưa áp trong XAML**
- [ ] Sender name hiển thị trong bubble (Group chat)

### Phase 4 — Input Area
- [x] `TextBox` placeholder bằng `GotFocus`/`LostFocus`
- [x] Toolbar icons: File 📎, Image 🖼 (có handler đầy đủ)
- [x] Send on Enter, Shift+Enter = newline
- [ ] Emoji Picker 😊 — **button có, handler trống**
- [ ] Bold / Italic / Strikethrough — **chưa có**

### Phase 5 — SignalR Integration
- [x] `SignalRService` — connect / disconnect với `WithAutomaticReconnect()`
- [x] HubUrl đọc từ `appsettings.json` — **đã fix hardcode**
- [x] Subscribe: `ReceiveMessage`, `UserTyping`, `UserOnlineStatus`
- [x] Invoke: `SendMessage`, `JoinGroup`, `LeaveGroup`, `NotifyTyping`
- [x] Typing debounce 1.5s bằng timestamp trong `OnInputTextChanged`
- [x] `JoinGroup` + `LeaveGroup` tự invoke khi `OnSelectedConversationChanged`
- [ ] `MessageRead` subscribe + broadcast — **chưa implement**

### Phase 6 — Polish
- [ ] Slide-in Storyboard animation cho bubble mới
- [ ] Read receipt (✓ → ✓✓ → xanh) hiển thị UI
- [ ] Notification sound khi có tin ngoài focus
- [ ] Window title badge `[3] NexChat`

---

## 🐛 Known Issues *(cần fix)*

### 🔴 Issue 1: Double Event Subscription — tin nhắn đến xử lý 2 lần

**Files:** `MainViewModel.cs` dòng 50–52 và `MainWindow.xaml.cs` dòng 36–38

```csharp
// MainViewModel.cs constructor — subscriber #1
_chatController.OnMessageReceived += ChatController_OnMessageReceived;

// MainWindow.xaml.cs MainWindow_Loaded — subscriber #2 (THỪA)
_chatController.OnMessageReceived += ChatController_OnMessageReceived;
```

**Fix:** Xóa 3 dòng subscription trong `MainWindow.MainWindow_Loaded`, để `MainViewModel` tự handle toàn bộ.

---

### 🔴 Issue 2: `MessageTemplateSelector` cast sai type

**File:** `MessageTemplateSelector.cs` dòng 14

```csharp
// SAI — item là MessageViewModel nhưng cast thành Message
if (item is Message message && App.CurrentUser != null)

// ĐÚNG — cast sang MessageViewModel
if (item is MessageViewModel message && App.CurrentUser != null)
{
    return message.IsSelf ? SelfMessageTemplate : OtherMessageTemplate;
}
```

**Hậu quả:** Selector luôn fall-through → tất cả bubble dùng default template, Self/Other không phân biệt được về `CornerRadius` và layout.

---

### 🟡 Issue 3: `MainViewModel` không unsubscribe event khi Window đóng

**Fix:** Unsubscribe trong `MainWindow.MainWindow_Closed`:

```csharp
private async void MainWindow_Closed(object? sender, EventArgs e)
{
    _chatController.OnMessageReceived -= ViewModel./* handler method */;
    _chatController.OnUserTyping -= ...;
    _chatController.OnUserOnlineStatusChanged -= ...;
    await _chatController.DisconnectRealtimeAsync();
}
```

---

### 🟡 Issue 4: SHA256 không có salt

`UserService.HashPassword` dùng SHA256 thuần — đủ cho demo assignment, không production-safe.

**Upgrade sau:** Cài `BCrypt.Net-Next` và dùng `BCrypt.Net.BCrypt.HashPassword()`.

---

## 🔌 Dependencies

| Package | Version | Dùng cho |
|---------|---------|----------|
| `Microsoft.AspNetCore.SignalR.Client` | 8.x | Real-time SignalR |
| `CommunityToolkit.Mvvm` | 8.x | `ObservableObject`, `[ObservableProperty]`, `RelayCommand` |
| `Microsoft.Extensions.DependencyInjection` | 8.x | DI container |
| `Microsoft.Extensions.Configuration` | 8.x | Đọc `appsettings.json` (HubUrl) |
| `Microsoft.EntityFrameworkCore.SqlServer` | 8.x | Code First — SQL Server |
| `System.Text.Json` | built-in | Parse attachment metadata JSON từ SignalR |
| `Newtonsoft.Json` | 13.x | JSON serialization (dự phòng) |

---

*Last updated: 2026-04-18 · NexChat WPF Design Spec v1.1 — cập nhật Implementation Status & Known Issues*
