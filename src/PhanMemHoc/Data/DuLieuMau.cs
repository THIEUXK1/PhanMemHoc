using Microsoft.AspNetCore.Identity;
using PhanMemHoc.Models;

namespace PhanMemHoc.Data;

public static class DuLieuMau
{
    public const string EmailAdmin = "admin@phanmemhoc.local";
    public const string MatKhauAdmin = "Admin@123";

    public static void KhoiTao(AppDb db, IPasswordHasher<PhuHuynh> hasher, ILogger logger)
    {
        // Bảng học viên của phiên bản cũ không còn dùng
        if (db.Db.CollectionExists("hoc_vien")) db.Db.DropCollection("hoc_vien");

        if (!db.PhuHuynh.Exists(x => x.LaAdmin))
        {
            var admin = new PhuHuynh { HoTen = "Quản trị", Email = EmailAdmin, LaAdmin = true };
            admin.MatKhauHash = hasher.HashPassword(admin, MatKhauAdmin);
            db.PhuHuynh.Insert(admin);
            logger.LogWarning("Đã tạo tài khoản quản trị {Email} / {MatKhau} — hãy đổi mật khẩu.", EmailAdmin, MatKhauAdmin);
        }

        if (db.BaiHoc.Count() == 0) db.BaiHoc.InsertBulk(BaiHocMau());
        else
        {
            // DB cũ: chỉ thêm các bài mẫu mới (theo tiêu đề) còn thiếu, không đụng bài admin đã sửa
            var thieu = BaiHocBoSung().Where(b => !db.BaiHoc.Exists(x => x.TieuDe == b.TieuDe)).ToList();
            if (thieu.Count > 0) db.BaiHoc.InsertBulk(thieu);
        }
    }

    private static CauHoi C(string deBai, string hien, int dapAn, params string[] luaChon) =>
        new() { Loai = LoaiCauHoi.Chon, DeBai = deBai, Hien = hien, DapAn = dapAn, LuaChon = [.. luaChon] };

    private static CauHoi G(string deBai, params string[] cap) =>
        new() { Loai = LoaiCauHoi.Ghep, DeBai = deBai, Cap = [.. cap] };

    private static CauHoi N(string deBai, params string[] cap) =>
        new() { Loai = LoaiCauHoi.Noi, DeBai = deBai, Cap = [.. cap] };

    private static CauHoi X(string deBai, params string[] thuTu) =>
        new() { Loai = LoaiCauHoi.SapXep, DeBai = deBai, LuaChon = [.. thuTu] };

    private static BaiHoc B(string nangLuc, int thuTu, string tieuDe, string icon, int tuoiTu, params CauHoi[] cau) =>
        new() { NangLuc = nangLuc, ThuTu = thuTu, TieuDe = tieuDe, Icon = icon, TuoiTu = tuoiTu, TuoiDen = 6, CauHoi = [.. cau] };

    public static List<BaiHoc> BaiHocMau() =>
    [
        B("so", 1, "Đếm đến 3", "🍎", 3,
            C("Có mấy quả táo?", "🍎🍎", 1, "1", "2", "3"),
            C("Có mấy chú vịt?", "🦆🦆🦆", 0, "3", "2", "4"),
            C("Có mấy ngôi sao?", "⭐", 1, "2", "1", "3"),
            C("Có mấy quả bóng?", "⚽⚽⚽", 2, "2", "4", "3")),
        B("so", 2, "Đếm đến 5", "🐟", 3,
            C("Có mấy con cá?", "🐟🐟🐟🐟", 0, "4", "5", "3"),
            C("Có mấy bông hoa?", "🌸🌸🌸🌸🌸", 2, "4", "6", "5"),
            C("Nhóm nào có 3 con mèo?", "", 1, "🐱🐱", "🐱🐱🐱", "🐱🐱🐱🐱"),
            C("Có mấy cái kẹo?", "🍬🍬🍬🍬🍬", 0, "5", "3", "4")),
        B("so", 3, "Đếm đến 10", "🎈", 4,
            C("Có mấy quả bóng bay?", "🎈🎈🎈🎈🎈🎈", 1, "7", "6", "5"),
            C("Có mấy con bướm?", "🦋🦋🦋🦋🦋🦋🦋🦋", 0, "8", "9", "7"),
            C("Có mấy quả dâu?", "🍓🍓🍓🍓🍓🍓🍓🍓🍓🍓", 1, "9", "10", "8"),
            C("Số nào đứng ngay sau số 6?", "6 ➡️ ❓", 2, "5", "8", "7"),
            C("Có mấy con gà?", "🐔🐔🐔🐔🐔🐔🐔", 0, "7", "6", "8")),

        B("hinh", 1, "Màu sắc", "🎨", 3,
            C("Bé chọn quả màu đỏ nhé!", "", 1, "🍌", "🍎", "🍇"),
            C("Cái nào màu vàng?", "", 0, "🌞", "🍆", "🥦"),
            C("Con vật nào màu xanh lá?", "", 0, "🐸", "🐷", "🐻"),
            C("Quả nào màu tím?", "", 2, "🍊", "🍋", "🍇")),
        B("hinh", 2, "Hình dạng", "🔺", 3,
            C("Bé chọn hình tròn nhé!", "", 1, "🔺", "⚪", "🟦"),
            C("Hình nào là hình vuông?", "", 0, "🟥", "🔺", "⚪"),
            C("Hình nào là hình tam giác?", "", 2, "⚪", "🟩", "🔺"),
            C("Quả bóng có dạng hình gì?", "⚽", 0, "Hình tròn", "Hình vuông", "Hình tam giác")),
        B("hinh", 3, "Hình và màu", "🟢", 4,
            C("Chọn hình tròn màu xanh lá.", "", 1, "🔴", "🟢", "🟩"),
            C("Chọn hình vuông màu vàng.", "", 0, "🟨", "🟡", "🟧"),
            C("Chọn hình tròn màu xanh dương.", "", 0, "🔵", "🟦", "🟣"),
            C("Chọn hình vuông màu đỏ.", "", 1, "🔴", "🟥", "🟪")),

        B("sosanh", 1, "To và nhỏ", "🐘", 3,
            C("Con vật nào to nhất?", "", 1, "🐭", "🐘", "🐱"),
            C("Con vật nào nhỏ nhất?", "", 0, "🐜", "🐶", "🐴"),
            C("Quả nào to hơn?", "", 1, "🍒", "🍉"),
            C("Con nào nhỏ hơn?", "", 1, "🐳", "🐟")),
        B("sosanh", 2, "Nhiều hơn, ít hơn", "⚖️", 4,
            C("Nhóm nào có nhiều táo hơn?", "", 1, "🍎🍎", "🍎🍎🍎🍎"),
            C("Nhóm nào có ít sao hơn?", "", 1, "⭐⭐⭐", "⭐"),
            C("Nhóm nào có nhiều cá nhất?", "", 1, "🐟🐟", "🐟🐟🐟🐟🐟", "🐟🐟🐟"),
            C("Số nào lớn hơn?", "", 1, "3", "5")),

        B("quyluat", 1, "Quy luật hình", "🔁", 3,
            C("Hình tiếp theo là gì?", "🔴🔵🔴🔵🔴❓", 1, "🔴", "🔵", "🟢"),
            C("Hình tiếp theo là gì?", "🟡🟡🟢🟡🟡❓", 1, "🟡", "🟢", "🔴"),
            C("Hình tiếp theo là gì?", "⭐🌙⭐🌙❓", 1, "🌙", "⭐", "☀️"),
            C("Quả tiếp theo là gì?", "🍎🍌🍎🍌❓", 0, "🍎", "🍌", "🍇")),
        B("quyluat", 2, "Quy luật số", "🔢", 5,
            C("Số tiếp theo là số mấy?", "1  2  3  4  ❓", 1, "6", "5", "7"),
            C("Số tiếp theo là số mấy?", "2  4  6  ❓", 2, "7", "9", "8"),
            C("Số tiếp theo là số mấy?", "5  4  3  ❓", 0, "2", "1", "4"),
            C("Số tiếp theo là số mấy?", "1  3  5  ❓", 1, "6", "7", "8")),

        B("phanloai", 1, "Tìm vật khác loại", "🧺", 3,
            C("Cái nào không phải trái cây?", "", 1, "🍎", "🚗", "🍌"),
            C("Cái nào không phải con vật?", "", 2, "🐶", "🐱", "⚽"),
            C("Cái nào không phải xe?", "", 1, "🚗", "🍰", "🚌"),
            C("Con nào không biết bay?", "", 2, "🐦", "🦋", "🐢")),
        B("phanloai", 2, "Xếp nhóm", "🗂️", 4,
            C("Con nào sống dưới nước?", "", 1, "🐔", "🐟", "🐄"),
            C("Cái nào dùng để ăn cơm?", "", 2, "👟", "🧢", "🥄"),
            C("Bé mặc gì khi trời lạnh?", "", 0, "🧥", "🩳", "👙"),
            C("Cái nào là rau củ?", "", 1, "🍭", "🥕", "🍩")),

        B("trinho", 1, "Lật thẻ con vật", "🐶", 3,
            G("Bé lật thẻ tìm các cặp con vật giống nhau nhé!", "🐶", "🐱", "🐰"),
            G("Tìm các cặp giống nhau nào!", "🐸", "🐵", "🐷", "🐔")),
        B("trinho", 2, "Lật thẻ trái cây", "🍓", 4,
            G("Bé tìm các cặp trái cây giống nhau nhé!", "🍎", "🍌", "🍇", "🍓"),
            G("Thử thách khó hơn nào!", "🍉", "🍍", "🍒", "🥝", "🍑")),
        .. BaiHocBoSung(),
    ];

    /// <summary>Bài mẫu dùng trò nối đôi / xếp thứ tự — được thêm vào cả DB đã có dữ liệu.</summary>
    public static List<BaiHoc> BaiHocBoSung() =>
    [
        B("so", 4, "Xếp số", "🪜", 4,
            X("Bé xếp các số từ bé đến lớn nhé!", "1", "2", "3"),
            N("Bé nối số với nhóm quả dâu đúng nhé!", "1=🍓", "2=🍓🍓", "3=🍓🍓🍓"),
            X("Xếp từ ít đến nhiều nào!", "🍎", "🍎🍎", "🍎🍎🍎", "🍎🍎🍎🍎"),
            X("Bé xếp các số từ bé đến lớn nhé!", "2", "3", "4", "5")),
        B("hinh", 4, "Nối màu", "🖍️", 3,
            N("Bé nối hình tròn với đồ vật cùng màu nhé!", "🔴=🍎", "🟡=🍌", "🟢=🥦"),
            N("Nối màu giống nhau nào!", "🔴=🍓", "🟠=🥕", "🟣=🍇", "🔵=🐳")),
        B("sosanh", 3, "Từ nhỏ đến to", "🐭", 3,
            X("Bé xếp các con vật từ nhỏ đến to nhé!", "🐜", "🐭", "🐘"),
            X("Xếp từ nhỏ đến to nào!", "🐞", "🐱", "🐴", "🐳")),
        B("quyluat", 3, "Cái gì trước, cái gì sau?", "🌱", 4,
            X("Bé xếp cây lớn lên nhé: hạt, mầm, rồi cây!", "🌰", "🌱", "🌳"),
            X("Xếp lại nào: quả trứng, gà con, rồi gà mẹ!", "🥚", "🐣", "🐔"),
            X("Xếp buổi trong ngày: sáng, trưa, tối!", "🌅", "☀️", "🌙")),
        B("phanloai", 3, "Nối đôi", "🔗", 3,
            N("Bé nối con vật với món ăn của nó nhé!", "🐶=🦴", "🐰=🥕", "🐵=🍌"),
            N("Nối đồ vật đi cùng nhau nào!", "🧦=👟", "✏️=📒", "🪥=🦷", "☂️=🌧️")),
        .. KhoBaiHoc.TatCa(),
    ];
}
