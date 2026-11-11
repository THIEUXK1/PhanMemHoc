# PhanMemHoc — core (riêng dự án; quy tắc chung ở ~/.claude/CLAUDE.md)

## Dự án
- "Bé Vui Học": app học cho trẻ; phụ huynh quản lý hồ sơ bé, lộ trình theo năng lực; admin quản lý bài học.
- .NET 10 Minimal API + LiteDB 5 (file `phanmemhoc.db`) + cookie auth · SPA JS thuần (ES module, hash router) · xUnit.
- Chưa có git, chưa có máy deploy.

## Phân bổ file
| Loại | Nơi |
|---|---|
| Route API (`/api/*`) | `src/PhanMemHoc/Endpoints/*Endpoints.cs` (extension `MapXxx`) |
| Nghiệp vụ thuần (tính sao, lộ trình, báo cáo, validate) | `Services/` — static, không truy cập DB |
| Entity / DTO | `Models/` · DTO cạnh endpoint dùng nó |
| Collection, index, truy vấn dùng chung | `Data/AppDb.cs` · seed `Data/DuLieuMau.cs` |
| UI | `wwwroot/js/{app,game,admin}.js` · tiện ích chung `core.js` · CSS `wwwroot/css/app.css` |
| Test | `tests/PhanMemHoc.Tests/` |

## Cấm tuyệt đối
- Logic nghiệp vụ trong endpoint/JS khi đặt được ở `Services/`; HTML dựng trong C#.
- Endpoint `/api/be/*` thiếu kiểm sở hữu (`db.LayBeCuaPhuHuynh`) → IDOR. Admin chỉ qua group `RequireRole("Admin")`.
- Chèn chuỗi người dùng vào HTML không qua `esc()`.
- Thêm gói NuGet/npm, bundler, framework JS khi chưa hỏi.
- Xoá/sửa `*.db` khi app đang chạy; drop collection mới trong `DuLieuMau` khi chưa hỏi.

## View / AJAX
- Mọi gọi API qua `api()` trong `core.js` (tự xử 401 → `#/dangnhap`, ném `ApiError`).
- Điều hướng bằng `location.hash`; cấm `location.reload()`, cấm form submit thật (`preventDefault`).
- Code mới: event delegation trên `document`/root view; lỗi → `toast()`.

## Code style
- Tên miền tiếng Việt không dấu PascalCase (`HoSoBe`, `TinhSao`); collection snake_case (`ho_so_be`).
- Endpoint trả `Results.*`; lỗi validate → `Results.BadRequest("thông điệp tiếng Việt")`.
- Nullable bật — không `!` để im cảnh báo.
- Chi tiết: `docs/coding-standards.md`.

## Lệnh (chạy từ gốc repo)
- Build: `dotnet build -v q -clp:ErrorsOnly`
- Test: `dotnet test -v q --nologo 2>&1 | grep -E "Failed|error|Passed!|Failed!" | head -n 20`
- Hot reload (nền): `dotnet watch --non-interactive --project src/PhanMemHoc > watch.log 2>&1` → `tail -n 30 watch.log`
- LiteDB `Connection=direct` khoá file: dừng watcher trước khi chạy instance thứ hai.

## Task → đọc doc nào (`.claude/docs/`)
| Task | Doc |
|---|---|
| Thêm file/tính năng, quy trình thay đổi, gọi subagent | `architecture-workflow.md` |
| Viết code C#/JS | `coding-standards.md` |
| Cây thư mục, route, cạm bẫy, lệnh | `dotnet-architecture.md` |
| Đổi model/collection, seed, xoá dữ liệu | `database-safety.md` |
| Tính năng mới có thuộc phạm vi không | `project-scope.md` |
| Bắt đầu task | `.claude/plans/00-context-memory.md` |

## Subagent (chỉ khi được yêu cầu)
- `system-architect`: đổi stack/cấu trúc · `database-auditor`: đổi model/seed/xoá dữ liệu · `code-reviewer`: trước khi chốt tính năng.
