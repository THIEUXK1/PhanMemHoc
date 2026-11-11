# Kiến trúc .NET

## Cây thư mục
```
PhanMemHoc.slnx
src/PhanMemHoc/
  Program.cs            DI, cookie auth, static files, Map*()
  Data/AppDb.cs         collection + index + truy vấn dùng chung
  Data/DuLieuMau.cs     seed admin + bài mẫu lúc khởi động
  Endpoints/            Auth (/api/auth), Hoc (/api/nangluc, /api/be), Admin (/api/admin)
  Models/               BaiHoc+CauHoi, HoSoBe, KetQua, NangLuc (danh mục cố định), PhuHuynh
  Services/LoTrinh.cs   tính sao, chấm điểm, lộ trình, báo cáo, validate bài
  wwwroot/              index.html, css/app.css, js/{core,app,game,admin}.js
tests/PhanMemHoc.Tests/ xUnit — LoTrinhTests
```

## Phụ thuộc
- `LiteDB 5.0.21` — DB nhúng 1 file, không cần server.
- `Microsoft.AspNetCore.Identity` (có sẵn trong framework) — chỉ dùng `PasswordHasher`.
- Test: `xunit`, `Microsoft.NET.Test.Sdk`, `xunit.runner.visualstudio`, `coverlet.collector`.
- Frontend: Google Fonts (Baloo 2), không thư viện JS.

## Route
| Method | Path | Quyền |
|---|---|---|
| POST | `/api/auth/dangky`, `/dangnhap`, `/dangxuat` · GET `/api/auth/toi` | công khai / cookie |
| GET | `/api/nangluc` | công khai |
| GET/POST/PUT/DELETE | `/api/be`, `/api/be/{id}` | đăng nhập + sở hữu |
| GET | `/api/be/{id}/lotrinh`, `/baihoc/{baiId}`, `/baocao` | đăng nhập + sở hữu |
| POST | `/api/be/{id}/ketqua` | đăng nhập + sở hữu |
| GET/POST/PUT/DELETE | `/api/admin/baihoc[/{id}]` · GET `/api/admin/thongke` | role Admin |

Frontend hash route: `#/dangnhap`, `#/`, `#/be/{id}`, `#/be/{id}/hoc/{baiId}`, `#/be/{id}/baocao`, `#/admin`.

## Quy ước
- Auth cookie `phanmemhoc.auth` (HttpOnly, SameSite=Strict, 30 ngày trượt); 401/403 trả mã, không redirect.
- Role Admin = claim từ `PhuHuynh.LaAdmin`.

## Cạm bẫy đã cắn
- DB nằm ở `ContentRootPath` → chạy `dotnet run --project src/PhanMemHoc` dùng `src/PhanMemHoc/phanmemhoc.db`; chạy từ chỗ khác tạo DB mới (đã có `phanmemhoc.db` lạc ở gốc repo).
- `Connection=direct` khoá file độc quyền: hai instance (watch + run) → lỗi file đang dùng.
- `phanmemhoc-log.db` là journal LiteDB — không xoá khi app đang chạy.
- Mật khẩu admin seed hardcode trong `DuLieuMau.cs` (`EmailAdmin`/`MatKhauAdmin`) — chưa có `appsettings*.json`; cần chuyển sang config/user-secrets trước khi deploy (người dùng quyết).
- `DuLieuMau.KhoiTao` drop collection `hoc_vien` mỗi lần khởi động (dọn bản cũ).

## Lệnh hay dùng
- Lệnh build/test/watch: xem `rules/core.md`.
- Chạy 1 lần: `dotnet run --project src/PhanMemHoc`
- Test một lớp: `dotnet test --filter "FullyQualifiedName~LoTrinhTests" -v q`

## Deploy & SSH
- Chưa có. Khi có: thêm vào đây + `settings.local.json` (SSH key, không mật khẩu).
