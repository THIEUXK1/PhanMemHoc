# An toàn dữ liệu (LiteDB)

## Môi trường & chủ dữ liệu
- Chỉ có máy dev: `src/PhanMemHoc/phanmemhoc.db` (+ journal `phanmemhoc-log.db`). Dữ liệu là của người dùng — không tự xoá/tạo lại.
- Chưa có production. Khi có: mọi lệnh GHI production luôn hỏi từng lần (AskUserQuestion), kể cả đã có allow rule.

## Quy tắc tuyệt đối
- Không xoá, ghi đè, đổi tên file `*.db` khi chưa hỏi. Muốn reset → đề xuất backup trước (`cp phanmemhoc.db phanmemhoc.db.bak` khi app đã dừng).
- Không thêm `DropCollection`/`DeleteMany` mới vào `DuLieuMau` khi chưa hỏi.
- Xoá dữ liệu hiện là hard delete (`XoaBe` xoá cả `KetQua`; xoá `BaiHoc` xoá `KetQua` liên quan). Đổi sang soft-delete là quyết định của người dùng.

## Đổi schema (không có migration tool)
- LiteDB schemaless: thêm property mới → document cũ nhận giá trị mặc định; đặt default an toàn trong class.
- Đổi tên/xoá property, đổi kiểu → cần đoạn chuyển đổi idempotent trong `DuLieuMau.KhoiTao`, kèm cách quay lại (down) mô tả trong PR; chạy lại nhiều lần không hỏng.
- Seed idempotent: luôn kiểm `Exists`/`Count` trước `Insert` (mẫu admin hiện có).
- Index mới thêm trong constructor `AppDb` (`EnsureIndex` idempotent).

## Truy vấn an toàn
- Dùng LINQ expression của LiteDB (`Find(x => ...)`), không ghép chuỗi `BsonExpression`.
- Truy vấn theo bé luôn qua `LayBeCuaPhuHuynh(beId, phuHuynhId)` trước.
- Tránh `FindAll()` + lọc trong bộ nhớ trên collection lớn (`KetQua`); dùng index `BeId`/`BaiHocId`.
- Báo cáo thay đổi dữ liệu: nêu collection + số document bị chạm + cách khôi phục.
