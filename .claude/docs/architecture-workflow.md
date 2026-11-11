# Kiến trúc & quy trình làm việc

## Git flow
- Repo CHƯA `git init`. Khi init: nhánh `main` ổn định, mỗi tính năng một nhánh `feat/<ten>`; chỉ commit khi người dùng yêu cầu.
- `.gitignore` đã chặn `*.db`, `*-log.db`, `bin/`, `obj/`, `*.log`.

## Tạo file ở đâu
| Muốn thêm | Tạo/sửa | Ghi chú |
|---|---|---|
| Nhóm API mới | `Endpoints/<Ten>Endpoints.cs` với `public static void MapTen(this WebApplication app)` | Gọi `app.MapTen()` trong `Program.cs` |
| Route vào nhóm có sẵn | thêm vào `MapGroup` tương ứng | `/api/be/*` cần auth + kiểm sở hữu; `/api/admin/*` role Admin |
| Entity mới | `Models/<Ten>.cs` + property `ILiteCollection<T>` + index trong `AppDb` | Đọc `database-safety.md` trước |
| Tính toán / validate | `Services/` (static, nhận dữ liệu đã nạp) | Viết test trong `tests/` |
| DTO request | `record` cạnh endpoint dùng nó | |
| Màn hình phụ huynh/bé | `wwwroot/js/app.js` (route trong mảng `routes`) | |
| Trò chơi / làm bài | `wwwroot/js/game.js` | |
| Màn quản trị | `wwwroot/js/admin.js` | |
| Tiện ích JS chung | `wwwroot/js/core.js` (export) | |
| Style | `wwwroot/css/app.css` | Không thêm file CSS mới khi chưa cần |

## Quy trình mỗi thay đổi
1. Đọc `plans/00-context-memory.md`.
2. Grep vị trí → đọc đúng đoạn.
3. Nghiệp vụ mới: viết/sửa ở `Services/` + test.
4. Endpoint: validate input, kiểm quyền/sở hữu, trả `Results.*`.
5. UI: gọi `api()`, escape bằng `esc()`, cập nhật DOM cục bộ.
6. `dotnet build` + `dotnet test` (lệnh kẹp ở `rules/core.md`); xem UI qua watcher.
7. Cập nhật `plans/00-context-memory.md` nếu task dở.

## Khi nào gọi subagent (chỉ khi người dùng yêu cầu)
- `system-architect`: thêm tầng/thư viện, đổi LiteDB, tách SPA, thêm deploy.
- `database-auditor`: đổi entity/collection, sửa `DuLieuMau`, thêm xoá dữ liệu.
- `code-reviewer`: xong một tính năng, trước khi commit.
