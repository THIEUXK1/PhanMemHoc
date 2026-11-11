# Decision log

Định dạng: `YYYY-MM-DD — quyết định — lý do — hệ quả`

- (có sẵn trong code) — LiteDB nhúng 1 file, `Connection=direct` — app nhỏ, không cần DB server — chỉ một process mở DB.
- (có sẵn trong code) — SPA JS thuần + hash router, không bundler — không build step frontend — không dùng thư viện UI.
- (có sẵn trong code) — Cookie auth, 401/403 trả mã thay redirect — SPA tự điều hướng — client xử 401 trong `api()`.
