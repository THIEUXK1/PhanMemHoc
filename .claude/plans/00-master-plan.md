# Master plan

## Phase hiện tại — MVP đã chạy
- [x] Auth phụ huynh, hồ sơ bé, lộ trình, làm bài, báo cáo, quản trị bài học.

## Việc mở (chưa xếp phase — người dùng quyết)
- [ ] `git init` + commit đầu.
- [ ] Chuyển mật khẩu admin seed ra config (user-secrets / `appsettings.Development.json` gitignore).
- [ ] Xử lý `phanmemhoc.db` lạc ở gốc repo.
- [ ] Phương án deploy.

## Checklist kiểm thử tay (mỗi lần đổi UI/API)
- [ ] Đăng ký → đăng nhập → đăng xuất; vào `#/` khi chưa đăng nhập → về `#/dangnhap`.
- [ ] Tạo/sửa/xoá hồ sơ bé; không xem được bé của tài khoản khác (đổi id trên URL).
- [ ] Làm một bài → sao cập nhật, bài kế mở khoá.
- [ ] Báo cáo hiển thị đúng sau khi học.
- [ ] Admin: CRUD bài học, validate báo lỗi; tài khoản thường vào `#/admin` bị chặn.
- [ ] Mobile width ~375px không vỡ layout.
