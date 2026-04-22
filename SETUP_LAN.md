# Hướng dẫn Setup App WPF chạy qua mạng LAN (Cùng Wifi)

Tài liệu này hướng dẫn cách cấu hình máy chủ (Server) và các máy trạm (Client) để ứng dụng WPF Chat có thể gọi chung 1 Database SQL Server và truyền file thành công qua mạng LAN nội bộ.

---

## 🔥 PHẦN 1: TẠI MÁY CHỦ (SERVER PC)

Máy này phải được **MỞ ĐẦU TIÊN** và luôn hoạt động để các máy khác có thể chui vào lấy dữ liệu.

### Bước 1: Lấy địa chỉ IP mạng LAN của máy chủ
1. Nhấn phím `Windows + R`, gõ `cmd` rồi Enter.
2. Trong màn hình đen, gõ lệnh `ipconfig` và nhấn Enter.
3. Tìm vùng mạng đang dùng (Thường là **Wireless LAN adapter Wi-Fi**), ghi lại số IP tại dòng **IPv4 Address**. 
   > Ví dụ: `192.168.1.50` (Đây là "địa chỉ nhà" của máy chủ).

### Bước 2: Cấu hình SQL Server (Bật TCP/IP & Tài khoản sa)
Mặc định SQL Server ở chế độ "kỉ bí", bạn phải ép nó mở cổng mạng TCP và dùng mật khẩu Server chứ không dùng Windows Authentication.

1. Mở app **SQL Server Configuration Manager** (Tìm trong Windows Start).
2. Bên cột trái, mở mục `SQL Server Network Configuration` > Chọn `Protocols for SQLEXPRESS`.
3. Nhìn bên cột phải:
   * Nếu **TCP/IP** đang là *Disabled*, click chuột phải chọn **Enable**.
   * Click chuột phải vào **TCP/IP** lần nữa > **Properties** > Tab **IP Addresses**.
   * Cuộn xuống dưới cùng mục **IPAll**. Ở ô `TCP Port`, gõ vào `1433`. Nhấn OK.
4. Bên cột trái, chọn **SQL Server Services** > Click phải vào dịch vụ **SQL Server (SQLEXPRESS)** > **Restart** (Để khởi động lại).
5. Mở phần mềm quản lý **SQL Server Management Studio (SSMS)** và đăng nhập.
6. Click chuột phải vào tên Server ở cùng (Cao nhất) > `Properties` > `Security` > Đổi chế độ lưu thành **SQL Server and Windows Authentication mode** > OK.
7. Mở rộng mục khóa `Security` > `Logins` > Click đúp vào tài khoản **sa**.
   * Cài Pass cho nó, ví dụ: `123456`.
   * Chuyển qua tab `Status`, mục Login chọn **Enabled** > OK. 
8. Click phải vào chữ tên Cục Server cao nhất 1 lần nữa > Chọn **Restart**.

### Bước 3: Mở khoá Tường lửa mạng (Windows Firewall)
1. Mở **Windows Defender Firewall** > Chọn **Advanced Settings**.
2. Chọn **Inbound Rules** > click **New Rule...** (bên góc phải).
3. Chọn **Port** > Next. Mặc định là TCP, gõ vào ô *Specific local ports*: 
   * `1433` (cho Database SQL).
   * `5000` (cho SignalR Server).
   > Bạn có thể gõ: `1433, 5000` > Nhấn Next.
4. Chọn **Allow the connection** > Next > Next. Đặt tên, ví dụ: `Cho app Chat` > Finish.

### Bước 4: Tạo "Kho Dùng Chung" để chứa File Upload
1. Ở máy chủ, ví dụ tạo 1 Folder tại ổ `D:\ChatUploads`.
2. Click chuột phải File đó > **Properties** > Tab **Sharing** > Click nút **Share...**
3. Ở khung chọn, kéo mũi tên xuống chọn **Everyone** > nhấn nút **Add**.
4. Chỉnh quyền (Permission Level) của Everyone thành **Read/Write**. Nhấn **Share**.
5. Bạn sẽ thấy sinh ra 1 cái link URL (Ví dụ: `\\192.168.1.50\ChatUploads`). Giữ link này lại.

### Bước 5: Tắt yêu cầu mật khẩu (QUAN TRỌNG)
Để máy khác không bị hỏi mật khẩu khi gửi file:
1. Vào **Control Panel** > **Network and Sharing Center** > **Change advanced sharing settings**.
2. Mở mục **All Networks**.
3. Ở dưới cùng, chọn **Turn off password protected sharing**.
4. Nhấn **Save changes**.

---

## 🛠 PHẦN 2: SỬA CODE WPF (Ở mọi máy tính)

Bây giờ bạn mở Visual Studio, mở Code App lên. Chúng ta cần vứt hết các chữ `localhost` đi và xài địa chỉ thật.

### 1. File cấu hình Database (`appsettings.json`)
Sửa thành y như này:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=192.168.1.50; Database=TênDB_CủaBạn; User Id=sa; Password=123456; TrustServerCertificate=True"
}
```
*(nhớ đổi bằng IP thật ở B1 và pass thật).*

### 2. File kết nối SignalR (`appsettings.json`)
Cấu hình URL của SignalR Hub để các máy khác có thể kết nối tới:
```json
"SignalR": {
  "HubUrl": "http://192.168.1.50:5000/chatHub"
}
```
*(Code đã được cập nhật để tự động lắng nghe trên cổng này và cho phép kết nối LAN).*

### 3. File nhận/gửi hình ảnh (`MessageService.cs`)
Tìm nơi lưu folder Uploads (vd biến `string uploadsFolder`), sửa thành đường dẫn chia sẻ mạng (nơi mà file bạn Shared ở Bước 4):
```csharp
string uploadsFolder = @"\\192.168.1.50\ChatUploads";
```

---

## 🚀 PHẦN 3: TEST KIỂM TRA

1. Toàn bộ máy tính (Máy Bạn, Máy Thầy, Máy Bạn Bè) phải cùng kết nối chung **1 mạng Wi-Fi cục bộ**.
2. **Ở Máy Server:** Chuột phải vô Visual Studio, nhấn Run App (hoặc Run API SignalR lên). Bắt buộc Server phải chạy thì máy khác mới chui vào được.
3. Cầm File `.exe` build ra USB -> Đem gắn qua và chạy ở máy khác. (Hoặc xách cả Solution Code qua máy khác ấn Run).
4. 🎉 Tận hưởng thành quả! Mọi Login, Chat, Upload Ảnh, xem Video đều mượt mà trơn tru như đang xài Zalo nội bộ!
