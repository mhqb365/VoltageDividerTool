# Phần mềm chia áp - Voltage Divider Tool

Một công cụ tính toán điện tử, dành cho các kỹ sư và những người đam mê điện tử. Dự án được xây dựng với giao diện Dark Mode và sơ đồ mạch trực quan

---

## Tính năng chính

### 1. Cầu phân áp (Voltage Divider)
- Tính toán điện áp tại các nút với số lượng điện trở tùy biến ($N$ điện trở)
- Hỗ trợ tính ngược: Nhập $V_{out}$ để tìm $V_{in}$ cần thiết
- Sơ đồ mạch động, tự động cập nhật giá trị linh kiện

### 2. Điện trở song song (Parallel Resistance)
- Tính giá trị tổng trở của hệ thống điện trở mắc song song
- Cho phép thêm/bớt số lượng điện trở không giới hạn

### 3. IC Ổn áp (IC Regulator)
- Chuyên dụng cho các dòng IC ổn áp như **LM1117 ADJ**
- Tính toán nhanh các tham số: $V_{ref}$, $R_1$, $R_2$, và $V_{out}$

### 4. Điện áp nút (Voltage Node)
- Phân tích nút điện áp phức tạp với 3 điện trở và 2 nguồn cấp độc lập
- Áp dụng định lý Millman để đưa ra kết quả chính xác nhất

### 5. Mã màu điện trở (Color Code)
- Hỗ trợ giải mã điện trở 4 vòng màu, 5 vòng màu và 6 vòng màu
- Tùy chọn sai số (Tolerance) và hệ số nhiệt (TempCo)
- Hiển thị mô phỏng điện trở thực tế cực kỳ bắt mắt

### 6. Mã điện trở dán (SMD Code)
- Giải mã nhanh các loại mã SMD 3 chữ số và 4 chữ số
- Hỗ trợ ký tự 'R' đại diện cho dấu phẩy thập phân (ví dụ: 4R7 = 4.7Ω)

---

## Công nghệ sử dụng

- **Framework:** .NET Framework 4.8
- **Platform:** WPF (Windows Presentation Foundation)
- **Language:** C#
- **Rendering:** Custom XAML Geometry & Canvas Drawing
- **Design:** Modern Dark Theme avec Glassmorphism elements

---

## Hướng dẫn sử dụng

1. **Chọn chế độ:** Sử dụng thanh Sidebar bên trái để chuyển đổi giữa các công cụ
2. **Nhập thông số:** Nhập các giá trị điện trở (Ω) hoặc điện áp (V) vào các ô tương ứng
3. **Tính toán:** Nhấn vào các nút tiêu đề (ví dụ: `Vout`, `R1`, `Vin`) để thực hiện tính toán giá trị đó dựa trên các thông số đã có
4. **Định dạng kết quả:** Kết quả sẽ tự động được làm tròn và thêm đơn vị (Ω, kΩ, MΩ) để dễ đọc
5. **Chọn ngôn ngữ:** Nhấp vào biểu tượng lá cờ (VI/EN) ở góc trên bên phải để chuyển đổi ngôn ngữ

---

## Tải xuống & Sử dụng

Nếu bạn chỉ muốn sử dụng phần mềm mà không cần quan tâm đến mã nguồn, hãy làm theo các bước sau:

1. Truy cập vào trang [Releases](https://github.com/mhqb365/VoltageDividerTool/releases)
2. Tìm phiên bản mới nhất (có nhãn **Latest**) và tải xuống tệp nén `.zip`
3. Giải nén tệp vừa tải về một thư mục bất kỳ trên máy tính
4. Chạy tệp `Phan mem chia ap.exe` hoặc `Voltage Divider Tool.exe` để dùng luôn mà không cần cài đặt (2 tệp này như nhau, tại thích để 2 tên vậy á 😁)

*Lưu ý: Máy tính của bạn cần được cài đặt .NET Framework 4.8 trở lên (thường đã có sẵn trên Windows 10/11)*

---

## Cài đặt & Phát triển

1. Clone repository về máy:
   ```bash
   git clone https://github.com/mhqb365/tinh-cau-phan-ap.git
   ```
2. Mở file giải pháp `.sln` hoặc tệp dự án `.csproj` bằng **Visual Studio 2022**
3. Đảm bảo bạn đã cài đặt gói **.NET Desktop Development**
4. Nhấn **F5** để Build và Run ứng dụng
5. Hoặc build bằng cách chạy file `build.bat`. File exe để Run ứng dụng sẽ nằm trong thư mục `dist`

---

## Tác giả

Phát triển bởi [mhqb365.com](https://mhqb365.com)

---
*Cảm ơn bạn đã sử dụng Voltage Divider Tool! Hy vọng công cụ này sẽ giúp ích cho công việc của bạn*
