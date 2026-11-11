# Phạm vi dự án

## Định nghĩa
Web app "Bé Vui Học": phụ huynh tạo hồ sơ bé, bé học bài tương tác theo lộ trình năng lực phù hợp tuổi, phụ huynh xem báo cáo; admin soạn bài học.

## In scope
- Tài khoản phụ huynh (đăng ký/đăng nhập cookie), nhiều hồ sơ bé.
- Bài học theo nhóm năng lực (`DanhMucNangLuc`), lọc theo tuổi, mở khoá tuần tự, tính sao.
- Báo cáo tiến bộ + gợi ý hoạt động thực tế.
- Trang quản trị CRUD bài học + thống kê.
- Giọng đọc (speechSynthesis), âm thanh, giao diện thân thiện trẻ em, mobile-first.

## Out of scope (hiện tại)
- Thanh toán, quảng cáo, chat, mạng xã hội.
- Tài khoản riêng cho bé (bé dùng qua tài khoản phụ huynh).
- App native, offline/PWA, đa ngôn ngữ.

## Do
- Giữ app chạy được bằng một lệnh, DB một file.
- Nội dung, thông báo bằng tiếng Việt có dấu, phù hợp trẻ nhỏ.

## Don't
- Không thu thập thêm dữ liệu cá nhân của trẻ ngoài hồ sơ hiện có.
- Không thêm dịch vụ ngoài (analytics, CDN JS) khi chưa hỏi.

## Vùng xám — phải hỏi
- Đổi LiteDB sang DB server, thêm framework frontend.
- Soft-delete / lưu lịch sử thay vì xoá cứng.
- Thêm vai trò mới (giáo viên, lớp học).
- Cấu hình deploy, tên miền, HTTPS.
