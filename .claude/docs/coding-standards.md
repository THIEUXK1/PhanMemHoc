# Chuẩn code

## C#
- File-scoped namespace `PhanMemHoc.<Thu muc>`; `ImplicitUsings`, `Nullable` bật.
- Tên miền tiếng Việt không dấu PascalCase: class/method/property (`HoSoBe`, `TinhSao`, `LayBeCuaPhuHuynh`).
- Biến cục bộ tiếng Việt camelCase (`duongDanDb`).
- Endpoint: lambda ngắn, inject `AppDb`, `ClaimsPrincipal`; logic > ~10 dòng → đưa sang `Services/`.
- `Services/` static, thuần (không DB, không `DateTime.Now` — nhận `bayGio` làm tham số) để test được.
- DTO là `record`; entity là `class` có `Id int` (LiteDB auto id).
- Trả về: `Results.Ok/NotFound/BadRequest/Forbid`; không ném exception cho lỗi nghiệp vụ.
- Validate trả `string?` lỗi (mẫu `LoTrinh.KiemTra`) → `Results.BadRequest(loi)`.

## Lỗi & dữ liệu rỗng
- Không tìm thấy hoặc không sở hữu → `NotFound` (không lộ tồn tại).
- Danh sách rỗng trả `[]`, không `null`.
- Không `catch {}` nuốt lỗi phía server; phía JS chỉ nuốt khi có fallback rõ (đã có comment).

## Frontend
- ES module, không bundler, không framework. Import từ `core.js`.
- View = hàm render chuỗi HTML vào root; mọi dữ liệu động qua `esc()`.
- Gọi API chỉ qua `api()`; lỗi → `toast(err.message)`.
- Điều hướng `location.hash`; không `location.reload()`.
- Code mới: event delegation (`root.addEventListener('click', e => e.target.closest('[data-x]'))`) thay cho gán `onclick` từng phần tử.
- Giao diện cho trẻ: nút to, emoji icon, giọng đọc `noi()`; giữ font Baloo 2.

## Comment
- Tiếng Việt, chỉ giải thích "tại sao"; XML `<summary>` cho method public của `AppDb`/`Services`.
