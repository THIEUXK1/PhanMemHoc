using PhanMemHoc.Models;

namespace PhanMemHoc.Data;

/// <summary>
/// Kho bài học sinh tự động cho dữ liệu mẫu. Dùng Random có seed cố định nên mỗi lần chạy ra cùng nội dung;
/// tiêu đề không đổi để seed chỉ chèn bài còn thiếu.
/// Thứ tự thêm trong mỗi nhóm = thứ tự mở khoá (dễ → khó, tuổi nhỏ → lớn).
/// </summary>
public static class KhoBaiHoc
{
    private const int SoCau = 5;

    public static List<BaiHoc> TatCa() => [.. So(), .. Hinh(), .. SoSanh(), .. QuyLuat(), .. PhanLoai(), .. TriNho(), .. ChuCai()];

    // ---------- Tiện ích ----------
    private sealed class Bo(string nangLuc, int thuTuDau)
    {
        public readonly List<BaiHoc> Ds = [];
        private int thuTu = thuTuDau;

        public void Them(string tieuDe, string icon, int tuoiTu, IEnumerable<CauHoi> cau) =>
            Ds.Add(new BaiHoc { NangLuc = nangLuc, ThuTu = thuTu++, TieuDe = tieuDe, Icon = icon, TuoiTu = tuoiTu, TuoiDen = 6, CauHoi = [.. cau] });

        public void Them(string tieuDe, string icon, int tuoiTu, Func<CauHoi> cau, int soCau = SoCau) =>
            Them(tieuDe, icon, tuoiTu, Enumerable.Range(0, soCau).Select(_ => cau()).ToList());
    }

    private static T Lay<T>(Random r, IReadOnlyList<T> ds) => ds[r.Next(ds.Count)];
    private static List<T> LayNhieu<T>(Random r, IEnumerable<T> ds, int n) => ds.OrderBy(_ => r.Next()).Take(n).ToList();
    private static string Hoa(string s) => s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s[1..];
    private static string Lap(string s, int n) => string.Concat(Enumerable.Repeat(s, n));
    private static List<int> LanCan(int n, int min, int max) => Enumerable.Range(n - 3, 7).Where(x => x != n && x >= min && x <= max).ToList();

    /// <summary>Câu chọn đáp án: 1 đáp án đúng + (soLuaChon − 1) đáp án sai lấy ngẫu nhiên, xáo trộn.</summary>
    private static CauHoi Chon(Random r, string deBai, string hien, string dung, IEnumerable<string> sai, int soLuaChon = 3)
    {
        var lc = LayNhieu(r, sai.Where(s => s != dung).Distinct(), soLuaChon - 1).Append(dung).OrderBy(_ => r.Next()).ToList();
        return new CauHoi { Loai = LoaiCauHoi.Chon, DeBai = deBai, Hien = hien, LuaChon = lc, DapAn = lc.IndexOf(dung) };
    }

    private static CauHoi Noi(string deBai, IEnumerable<(string Trai, string Phai)> cap) =>
        new() { Loai = LoaiCauHoi.Noi, DeBai = deBai, Cap = [.. cap.Select(c => $"{c.Trai}={c.Phai}")] };

    private static CauHoi XepThuTu(string deBai, IEnumerable<string> dungThuTu) =>
        new() { Loai = LoaiCauHoi.SapXep, DeBai = deBai, LuaChon = [.. dungThuTu] };

    private static CauHoi Ghep(string deBai, IEnumerable<string> hinh) =>
        new() { Loai = LoaiCauHoi.Ghep, DeBai = deBai, Cap = [.. hinh] };

    // ---------- Số và đếm ----------
    private static readonly (string E, string Ten)[] DoDem =
    [
        ("🍎", "quả táo"), ("🐟", "con cá"), ("🚗", "chiếc ô tô"), ("🐥", "chú gà con"), ("⭐", "ngôi sao"),
        ("🎈", "quả bóng bay"), ("🦋", "con bướm"), ("🍪", "cái bánh"), ("🌸", "bông hoa"), ("🐞", "con bọ rùa"),
        ("🍓", "quả dâu"), ("🐢", "con rùa"), ("⚽", "quả bóng"), ("🧸", "con gấu bông"), ("🍦", "cây kem"), ("🐝", "con ong"),
    ];

    private static readonly string[] ChuSo = ["không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín", "mười"];

    private static List<BaiHoc> So()
    {
        var b = new Bo("so", 10);
        var r = new Random(101);

        CauHoi Dem(int max)
        {
            var (e, ten) = Lay(r, DoDem);
            var n = r.Next(1, max + 1);
            return Chon(r, $"Có mấy {ten}?", Lap(e, n), $"{n}", LanCan(n, 1, max).Select(x => $"{x}"));
        }
        CauHoi NhomCo(int max)
        {
            var (e, ten) = Lay(r, DoDem);
            var n = r.Next(2, max + 1);
            return Chon(r, $"Nhóm nào có {ChuSo[n]} {ten}?", "", Lap(e, n), LanCan(n, 1, max).Select(x => Lap(e, x)));
        }
        CauHoi TimSo()
        {
            var n = r.Next(0, 11);
            return Chon(r, $"Bé tìm số {ChuSo[n]} nhé!", "", $"{n}", LanCan(n, 0, 10).Select(x => $"{x}"));
        }
        CauHoi NoiSo(int soCap, int max)
        {
            var (e, _) = Lay(r, DoDem);
            return Noi("Bé nối số với nhóm có đúng số lượng nhé!", LayNhieu(r, Enumerable.Range(1, max), soCap).Select(n => ($"{n}", Lap(e, n))));
        }
        CauHoi LienSau()
        {
            var n = r.Next(1, 10);
            return Chon(r, $"Số nào đứng ngay sau số {ChuSo[n]}?", $"{n} ➜ ❓", $"{n + 1}", [$"{n - 1}", $"{n}", $"{n + 2}"]);
        }
        CauHoi LienTruoc()
        {
            var n = r.Next(2, 11);
            return Chon(r, $"Số nào đứng ngay trước số {ChuSo[n]}?", $"❓ ➜ {n}", $"{n - 1}", [$"{n - 2}", $"{n}", $"{n + 1}"]);
        }
        CauHoi XepSo(int k, int max, bool tang)
        {
            var so = LayNhieu(r, Enumerable.Range(1, max), k).OrderBy(x => tang ? x : -x).Select(x => $"{x}");
            return XepThuTu(tang ? "Bé xếp các số từ bé đến lớn nhé!" : "Bé xếp các số từ lớn đến bé nhé!", so);
        }
        CauHoi CongHinh()
        {
            var (e, ten) = Lay(r, DoDem);
            var a = r.Next(1, 5);
            var c = r.Next(1, 6 - a);
            return Chon(r, $"Có {ChuSo[a]} {ten}, thêm {ChuSo[c]} nữa. Có tất cả mấy {ten}?", $"{Lap(e, a)} ➕ {Lap(e, c)}",
                $"{a + c}", LanCan(a + c, 1, 6).Select(x => $"{x}"));
        }
        CauHoi TruHinh()
        {
            var (e, ten) = Lay(r, DoDem);
            var a = r.Next(2, 6);
            var c = r.Next(1, a);
            return Chon(r, $"Có {ChuSo[a]} {ten}, bớt đi {ChuSo[c]}. Còn lại mấy {ten}?", $"{Lap(e, a)} ➖ {Lap(e, c)}",
                $"{a - c}", LanCan(a - c, 0, 5).Select(x => $"{x}"));
        }
        CauHoi Cong10()
        {
            var a = r.Next(1, 10);
            var c = r.Next(1, 11 - a);
            return Chon(r, $"{Hoa(ChuSo[a])} cộng {ChuSo[c]} bằng mấy?", $"{a} + {c} = ❓", $"{a + c}", LanCan(a + c, 0, 10).Select(x => $"{x}"));
        }
        CauHoi Tru10()
        {
            var a = r.Next(2, 11);
            var c = r.Next(1, a + 1);
            return Chon(r, $"{Hoa(ChuSo[a])} trừ {ChuSo[c]} bằng mấy?", $"{a} − {c} = ❓", $"{a - c}", LanCan(a - c, 0, 10).Select(x => $"{x}"));
        }

        for (var k = 1; k <= 4; k++) b.Them($"Đếm đến 5 – bài {k}", DoDem[k].E, 3, () => Dem(5));
        for (var k = 1; k <= 2; k++) b.Them($"Nối số – bài {k}", "🔗", 3, () => NoiSo(3, 5), 3);
        for (var k = 1; k <= 5; k++) b.Them($"Đếm đến 10 – bài {k}", DoDem[k + 5].E, 4, () => Dem(10));
        for (var k = 1; k <= 3; k++) b.Them($"Tìm nhóm đúng số – bài {k}", "👀", 4, () => NhomCo(7));
        for (var k = 1; k <= 3; k++) b.Them($"Nhận biết chữ số – bài {k}", "🔟", 4, TimSo);
        for (var k = 1; k <= 3; k++) b.Them($"Nối số đến 10 – bài {k}", "🧮", 4, () => NoiSo(4, 8), 3);
        for (var k = 1; k <= 3; k++) b.Them($"Xếp số từ bé đến lớn – bài {k}", "📶", 4, () => XepSo(k + 2, 10, true), 4);
        for (var k = 1; k <= 3; k++) b.Them($"Số liền trước, liền sau – bài {k}", "↔️", 5, () => r.Next(2) == 0 ? LienSau() : LienTruoc());
        for (var k = 1; k <= 2; k++) b.Them($"Xếp số từ lớn đến bé – bài {k}", "📉", 5, () => XepSo(k + 3, 10, false), 4);
        for (var k = 1; k <= 4; k++) b.Them($"Thêm vào là mấy? – bài {k}", "➕", 5, CongHinh);
        for (var k = 1; k <= 4; k++) b.Them($"Bớt đi còn mấy? – bài {k}", "➖", 5, TruHinh);
        for (var k = 1; k <= 4; k++) b.Them($"Phép cộng đến 10 – bài {k}", "🧠", 6, Cong10);
        for (var k = 1; k <= 4; k++) b.Them($"Phép trừ đến 10 – bài {k}", "🎯", 6, Tru10);
        for (var k = 1; k <= 3; k++) b.Them($"Cộng trừ hỗn hợp – bài {k}", "🏆", 6, () => r.Next(2) == 0 ? Cong10() : Tru10());
        return b.Ds;
    }

    // ---------- Hình và màu ----------
    private static readonly (string Ten, string O, string[] Vat)[] Mau =
    [
        ("đỏ", "🔴", ["🍎", "🍓", "🍅", "🌹", "🚒"]),
        ("vàng", "🟡", ["🍌", "🌻", "🐥", "🌽", "🧀"]),
        ("xanh lá", "🟢", ["🥦", "🐸", "🥒", "🍀", "🌲"]),
        ("xanh dương", "🔵", ["🐳", "💧", "🚙", "🧢", "🦋"]),
        ("cam", "🟠", ["🥕", "🍊", "🎃", "🦊", "🏀"]),
        ("tím", "🟣", ["🍇", "🍆", "🔮", "💜"]),
        ("nâu", "🟤", ["🐻", "🍫", "🌰", "🥔"]),
    ];

    private static readonly (string Ten, string[] E)[] HinhDang =
    [
        ("tròn", ["🔴", "🟢", "🔵", "🟡", "🟣"]),
        ("vuông", ["🟥", "🟩", "🟦", "🟨", "🟪"]),
        ("tam giác", ["🔺", "🔻"]),
        ("ngôi sao", ["⭐", "🌟"]),
        ("trái tim", ["❤️", "💛", "💚", "💙", "💜"]),
        ("thoi", ["🔷", "🔶"]),
    ];

    private static readonly (string Ten, string[] Vat)[] VatHinh =
    [
        ("tròn", ["⚽", "🍪", "🕐", "🍩", "🏀", "🍊"]),
        ("tam giác", ["🍕", "⛺", "🍙", "📐"]),
        ("vuông", ["🎁", "📦", "🧊", "🖼️"]),
    ];

    private static List<BaiHoc> Hinh()
    {
        var b = new Bo("hinh", 10);
        var r = new Random(202);

        CauHoi TimMau()
        {
            var m = Lay(r, Mau);
            return Chon(r, $"Cái nào màu {m.Ten}?", "", Lay(r, m.Vat), Mau.Where(x => x.Ten != m.Ten).SelectMany(x => x.Vat));
        }
        CauHoi MauGi()
        {
            var m = Lay(r, Mau);
            return Chon(r, "Hình này màu gì? Bé chọn chấm màu giống nhé!", Lay(r, m.Vat), m.O, Mau.Select(x => x.O));
        }
        CauHoi NoiMau(int soCap) =>
            Noi("Bé nối chấm màu với đồ vật cùng màu nhé!", LayNhieu(r, Mau, soCap).Select(m => (m.O, Lay(r, m.Vat))));
        CauHoi TimHinh()
        {
            var h = Lay(r, HinhDang);
            return Chon(r, $"Hình nào là hình {h.Ten}?", "", Lay(r, h.E), HinhDang.Where(x => x.Ten != h.Ten).SelectMany(x => x.E));
        }
        CauHoi NoiHinh(int soCap) =>
            Noi("Bé nối các hình có cùng hình dạng nhé!", LayNhieu(r, HinhDang, soCap).Select(h =>
            {
                var hai = LayNhieu(r, h.E, 2);
                return (hai[0], hai[1]);
            }));
        CauHoi VatCoHinh()
        {
            var v = Lay(r, VatHinh);
            return Chon(r, $"Đồ vật nào có hình {v.Ten}?", "", Lay(r, v.Vat), VatHinh.Where(x => x.Ten != v.Ten).SelectMany(x => x.Vat));
        }

        for (var k = 1; k <= 4; k++) b.Them($"Tìm màu – bài {k}", "🌈", 3, TimMau);
        for (var k = 1; k <= 3; k++) b.Them($"Màu gì đây? – bài {k}", "🎨", 3, MauGi);
        for (var k = 1; k <= 2; k++) b.Them($"Nối màu – bài {k}", "🖌️", 3, () => NoiMau(3), 3);
        for (var k = 1; k <= 4; k++) b.Them($"Tìm hình – bài {k}", "🔺", k <= 2 ? 3 : 4, TimHinh);
        for (var k = 1; k <= 3; k++) b.Them($"Nối hình giống nhau – bài {k}", "🔷", 4, () => NoiHinh(k + 2), 3);
        for (var k = 1; k <= 2; k++) b.Them($"Nối màu khó hơn – bài {k}", "🎨", 4, () => NoiMau(4), 3);
        for (var k = 1; k <= 3; k++) b.Them($"Đồ vật hình gì? – bài {k}", "🍕", 5, VatCoHinh);
        for (var k = 1; k <= 2; k++) b.Them($"Thử thách hình và màu – bài {k}", "🏅", 5, () => r.Next(3) switch { 0 => TimMau(), 1 => TimHinh(), _ => VatCoHinh() });
        return b.Ds;
    }

    // ---------- So sánh ----------
    private static readonly string[] TheoCo = ["🐜", "🐞", "🐭", "🐱", "🐶", "🐷", "🐴", "🐘", "🐳"];
    private static readonly (string Nang, string Nhe)[] NangNhe = [("🐘", "🐭"), ("🚚", "🎈"), ("🍉", "🍒"), ("🐻", "🐝"), ("🐄", "🐔"), ("🎹", "🪶")];
    private static readonly (string Nhanh, string Cham)[] NhanhCham = [("🐇", "🐢"), ("🐆", "🐌"), ("🚀", "🚲"), ("✈️", "🚗"), ("🐕", "🐛"), ("🐎", "🐄")];

    private static List<BaiHoc> SoSanh()
    {
        var b = new Bo("sosanh", 10);
        var r = new Random(303);

        /// Lấy k con vật cách nhau đủ xa về kích thước để bé phân biệt được.
        List<int> ChonCo(int k)
        {
            while (true)
            {
                var ds = LayNhieu(r, Enumerable.Range(0, TheoCo.Length), k).Order().ToList();
                if (ds.Zip(ds.Skip(1)).All(p => p.Second - p.First >= 2)) return ds;
            }
        }
        CauHoi NhieuIt(bool nhieu, int max)
        {
            var (e, _) = Lay(r, DoDem);
            var hai = LayNhieu(r, Enumerable.Range(1, max), 2);
            var dung = nhieu ? hai.Max() : hai.Min();
            return Chon(r, nhieu ? "Bên nào nhiều hơn?" : "Bên nào ít hơn?", "", Lap(e, dung), hai.Select(x => Lap(e, x)));
        }
        CauHoi NhieuNhat(bool nhieu)
        {
            var (e, _) = Lay(r, DoDem);
            var ba = LayNhieu(r, Enumerable.Range(1, 7), 3);
            return Chon(r, nhieu ? "Nhóm nào nhiều nhất?" : "Nhóm nào ít nhất?", "", Lap(e, nhieu ? ba.Max() : ba.Min()), ba.Select(x => Lap(e, x)));
        }
        CauHoi ToNho()
        {
            var ds = ChonCo(3);
            var to = r.Next(2) == 0;
            return Chon(r, to ? "Con nào to nhất?" : "Con nào nhỏ nhất?", "", TheoCo[to ? ds[^1] : ds[0]], ds.Select(i => TheoCo[i]));
        }
        CauHoi XepCo(int k, bool tang)
        {
            var ds = ChonCo(k).Select(i => TheoCo[i]);
            return XepThuTu(tang ? "Bé xếp các con vật từ nhỏ đến to nhé!" : "Bé xếp các con vật từ to đến nhỏ nhé!", tang ? ds : ds.Reverse());
        }
        CauHoi SoLon()
        {
            var ba = LayNhieu(r, Enumerable.Range(0, 11), 3);
            var lon = r.Next(2) == 0;
            return Chon(r, lon ? "Số nào lớn nhất?" : "Số nào bé nhất?", "", $"{(lon ? ba.Max() : ba.Min())}", ba.Select(x => $"{x}"));
        }
        CauHoi Nang()
        {
            var (nang, nhe) = Lay(r, NangNhe);
            return Chon(r, "Cái nào nặng hơn?", "", nang, [nhe], 2);
        }
        CauHoi Nhanh()
        {
            var (nhanh, cham) = Lay(r, NhanhCham);
            return Chon(r, "Cái nào đi nhanh hơn?", "", nhanh, [cham], 2);
        }

        for (var k = 1; k <= 3; k++) b.Them($"Nhiều hơn, ít hơn – bài {k}", "🍬", 3, () => NhieuIt(r.Next(2) == 0, 5));
        for (var k = 1; k <= 3; k++) b.Them($"To nhất, nhỏ nhất – bài {k}", "🐘", 3, ToNho);
        for (var k = 1; k <= 2; k++) b.Them($"Xếp con vật nhỏ đến to – bài {k}", "🐭", 3, () => XepCo(3, true), 4);
        b.Them("Cái nào nặng hơn?", "🏋️", 4, Nang);
        b.Them("Cái nào nhanh hơn?", "🐇", 4, Nhanh);
        for (var k = 1; k <= 3; k++) b.Them($"So sánh nhóm – bài {k}", "⚖️", 4, () => NhieuIt(r.Next(2) == 0, 9));
        for (var k = 1; k <= 3; k++) b.Them($"Số nào lớn hơn? – bài {k}", "🔢", 4, SoLon);
        for (var k = 1; k <= 2; k++) b.Them($"Xếp con vật to đến nhỏ – bài {k}", "🐳", 4, () => XepCo(k + 2, false), 4);
        for (var k = 1; k <= 3; k++) b.Them($"Nhiều nhất, ít nhất – bài {k}", "🧺", 5, () => NhieuNhat(r.Next(2) == 0));
        for (var k = 1; k <= 2; k++) b.Them($"Xếp 4 con vật – bài {k}", "🦒", 5, () => XepCo(4, r.Next(2) == 0), 4);
        for (var k = 1; k <= 2; k++) b.Them($"Thử thách so sánh – bài {k}", "🏅", 5, () => r.Next(4) switch { 0 => Nang(), 1 => Nhanh(), 2 => SoLon(), _ => NhieuNhat(true) });
        return b.Ds;
    }

    // ---------- Quy luật ----------
    private static readonly string[][] BoQuyLuat =
    [
        ["🍎", "🍌", "🍇"], ["🐶", "🐱", "🐰"], ["🔴", "🔵", "🟡"], ["🚗", "🚌", "🚲"], ["⭐", "❤️", "🌙"],
        ["🌸", "🍀", "🌻"], ["🐟", "🐙", "🦀"], ["☀️", "☁️", "⛄"], ["🟥", "🟦", "🟩"], ["🐸", "🐵", "🐷"],
    ];

    private static readonly (string DeBai, string[] Thu)[] TrinhTu =
    [
        ("Trứng nở ra sâu, sâu hoá thành bướm. Bé xếp lại nhé!", ["🥚", "🐛", "🦋"]),
        ("Bé xếp từ em bé lớn dần đến ông nhé!", ["👶", "🧒", "🧑", "👴"]),
        ("Xếp từ sáng sớm đến tối nhé!", ["🌅", "☀️", "🌇", "🌙"]),
        ("Mây kéo đến, trời mưa, rồi cầu vồng hiện ra!", ["☁️", "🌧️", "🌈"]),
        ("Trăng khuyết lớn dần thành trăng tròn!", ["🌑", "🌓", "🌕"]),
        ("Đi chợ, nấu ăn, rồi ăn cơm nào!", ["🛒", "🍳", "🍽️"]),
        ("Gieo hạt, cây nảy mầm, rồi hoa nở!", ["🌰", "🌱", "🌷"]),
        ("Buổi sáng: thức dậy, mặc áo, đeo cặp đi học!", ["🛏️", "👕", "🎒"]),
    ];

    private static List<BaiHoc> QuyLuat()
    {
        var b = new Bo("quyluat", 10);
        var r = new Random(404);
        string[] tatCaMau = ["AB", "AAB", "ABB", "ABC", "AABB"];

        CauHoi Mau(string mau, bool choTrong = false)
        {
            var bo = LayNhieu(r, Lay(r, BoQuyLuat), 3);
            var map = mau.Distinct().Select((c, i) => (c, e: bo[i])).ToDictionary(x => x.c, x => x.e);
            var dai = mau.Length * 2 + r.Next(mau.Length);
            var day = Enumerable.Range(0, dai + 1).Select(i => map[mau[i % mau.Length]]).ToList();
            if (!choTrong) return Chon(r, "Hình nào tiếp theo?", string.Join(" ", day.Take(dai)) + " ❓", day[dai], bo);
            var an = mau.Length + r.Next(dai - mau.Length);
            return Chon(r, "Hình nào còn thiếu?", string.Join(" ", day.Take(dai).Select((x, i) => i == an ? "❓" : x)), day[an], bo);
        }
        CauHoi DaySo()
        {
            var buoc = Lay(r, [1, 2, -1]);
            var dau = buoc > 0 ? r.Next(1, 5) : r.Next(5, 11);
            var day = Enumerable.Range(0, 4).Select(i => dau + i * buoc).ToList();
            return Chon(r, "Số nào tiếp theo?", string.Join(", ", day.Take(3)) + ", ❓", $"{day[3]}", LanCan(day[3], 0, 12).Select(x => $"{x}"));
        }

        for (var k = 1; k <= 3; k++) b.Them($"Quy luật AB – bài {k}", "🍎", 3, () => Mau("AB"));
        for (var k = 1; k <= 3; k++) b.Them($"Quy luật AAB – bài {k}", "🔁", 4, () => Mau(r.Next(2) == 0 ? "AAB" : "ABB"));
        b.Them("Trình tự quanh ta – bài 1", "🐛", 4, TrinhTu.Take(4).Select(t => XepThuTu(t.DeBai, t.Thu)));
        for (var k = 1; k <= 3; k++) b.Them($"Quy luật ABC – bài {k}", "🔄", 4, () => Mau("ABC"));
        b.Them("Trình tự quanh ta – bài 2", "🌷", 4, TrinhTu.Skip(4).Select(t => XepThuTu(t.DeBai, t.Thu)));
        for (var k = 1; k <= 2; k++) b.Them($"Quy luật AABB – bài {k}", "🔂", 5, () => Mau("AABB"));
        for (var k = 1; k <= 3; k++) b.Them($"Hình nào còn thiếu? – bài {k}", "❓", 5, () => Mau(Lay(r, tatCaMau), true));
        for (var k = 1; k <= 3; k++) b.Them($"Dãy số – bài {k}", "🔢", 5, DaySo);
        for (var k = 1; k <= 3; k++) b.Them($"Thử thách quy luật – bài {k}", "🏅", 6, () => r.Next(3) switch
        {
            0 => DaySo(),
            1 => Mau(Lay(r, tatCaMau), true),
            _ => Mau(Lay(r, tatCaMau)),
        });
        return b.Ds;
    }

    // ---------- Phân loại ----------
    /// <summary>Họ: các nhóm cùng họ chồng lấn nghĩa (chim cũng là con vật) nên không đem ra đánh đố lẫn nhau.</summary>
    private static readonly (string Ten, int Ho, string[] E)[] NhomDo =
    [
        ("con vật", 0, ["🐶", "🐱", "🐰", "🐻", "🐼", "🐸", "🐵", "🐷", "🐮", "🦁"]),
        ("loài chim", 0, ["🐦", "🦆", "🦉", "🐧", "🦜", "🐔", "🦅", "🕊️"]),
        ("con vật sống dưới nước", 0, ["🐟", "🐠", "🐙", "🦀", "🐬", "🐳", "🦈", "🦑"]),
        ("trái cây", 1, ["🍎", "🍌", "🍇", "🍓", "🍉", "🍊", "🍍", "🍒", "🍐", "🥭"]),
        ("rau củ", 1, ["🥕", "🥦", "🌽", "🥒", "🍆", "🥔", "🧅", "🥬"]),
        ("đồ ăn", 1, ["🍞", "🍚", "🥛", "🍕", "🍔", "🍜", "🥚", "🍦"]),
        ("xe cộ", 2, ["🚗", "🚌", "🚲", "🚂", "✈️", "🚁", "🚀", "⛵", "🚒", "🏍️"]),
        ("quần áo", 3, ["👕", "👖", "👗", "🧥", "🧦", "🧢", "👟", "🧤", "🧣", "👒"]),
        ("đồ chơi", 4, ["🧸", "⚽", "🎈", "🎲", "🧩", "🏀", "🛴", "🎯"]),
        ("đồ dùng học tập", 5, ["✏️", "📒", "📏", "✂️", "🖍️", "🎒", "📐", "📚"]),
        ("nhạc cụ", 6, ["🎸", "🥁", "🎹", "🎺", "🎻", "🎷"]),
    ];

    private static readonly (string DeBai, (string, string)[] Cap)[] BoNoi =
    [
        ("Bé nối con vật với món ăn của nó nhé!", [("🐶", "🦴"), ("🐰", "🥕"), ("🐵", "🍌"), ("🐭", "🧀"), ("🐱", "🐟"), ("🐝", "🌸"), ("🐼", "🎋"), ("🐄", "🌾")]),
        ("Bé nối con vật với nơi nó sống nhé!", [("🐟", "🌊"), ("🐦", "🌳"), ("🐪", "🏜️"), ("🐧", "🧊"), ("🐄", "🌾")]),
        ("Nối các đồ vật hay đi cùng nhau nào!", [("🧦", "👟"), ("✏️", "📒"), ("🔑", "🔒"), ("🥄", "🍲"), ("⚽", "🥅"), ("☂️", "🌧️"), ("🕶️", "☀️"), ("🧤", "❄️")]),
        ("Bé nối mỗi người với đồ dùng của họ nhé!", [("👨‍🍳", "🍳"), ("🧑‍🚒", "🚒"), ("🧑‍⚕️", "💉"), ("🧑‍🌾", "🌾"), ("👮", "🚓"), ("🧑‍🏫", "📚"), ("🧑‍🎨", "🎨")]),
    ];

    private static List<BaiHoc> PhanLoai()
    {
        var b = new Bo("phanloai", 10);
        var r = new Random(505);

        CauHoi KhacLoai()
        {
            var a = Lay(r, NhomDo);
            var khac = Lay(r, NhomDo.Where(x => x.Ho != a.Ho).ToList());
            var le = Lay(r, khac.E);
            var lc = LayNhieu(r, a.E, 3).Append(le).OrderBy(_ => r.Next()).ToList();
            return new CauHoi { Loai = LoaiCauHoi.Chon, DeBai = "Cái nào không cùng nhóm với các bạn còn lại?", LuaChon = lc, DapAn = lc.IndexOf(le) };
        }
        CauHoi LaLoai()
        {
            var a = Lay(r, NhomDo);
            return Chon(r, $"Cái nào là {a.Ten}?", "", Lay(r, a.E), NhomDo.Where(x => x.Ho != a.Ho).SelectMany(x => x.E));
        }
        CauHoi NoiDo(int soCap)
        {
            var (deBai, cap) = Lay(r, BoNoi);
            return Noi(deBai, LayNhieu(r, cap, soCap));
        }

        for (var k = 1; k <= 4; k++) b.Them($"Cái nào là…? – bài {k}", "🔎", 3, LaLoai);
        for (var k = 1; k <= 3; k++) b.Them($"Tìm bạn khác nhóm – bài {k}", "🧺", 3, KhacLoai);
        for (var k = 1; k <= 3; k++) b.Them($"Nối cho đúng – bài {k}", "🔗", 4, () => NoiDo(3), 3);
        for (var k = 1; k <= 3; k++) b.Them($"Phân loại đồ vật – bài {k}", "🗃️", 4, () => r.Next(2) == 0 ? KhacLoai() : LaLoai());
        for (var k = 1; k <= 3; k++) b.Them($"Nối khó hơn – bài {k}", "🧩", 5, () => NoiDo(4), 3);
        for (var k = 1; k <= 2; k++) b.Them($"Thử thách phân loại – bài {k}", "🏅", 5, KhacLoai);
        return b.Ds;
    }

    // ---------- Trí nhớ ----------
    private static readonly (string Ten, string Icon, string[] E)[] ChuDeThe =
    [
        ("biển", "🐠", ["🐟", "🐠", "🐙", "🦀", "🐬", "🐳"]),
        ("xe cộ", "🚗", ["🚗", "🚌", "🚲", "🚂", "✈️", "🚀"]),
        ("rau củ", "🥕", ["🥕", "🥦", "🌽", "🥒", "🍆", "🥔"]),
        ("đồ chơi", "🧸", ["🧸", "⚽", "🎈", "🎲", "🧩", "🪁"]),
        ("loài chim", "🦜", ["🐦", "🦆", "🦉", "🐧", "🦜", "🐔"]),
        ("quần áo", "👕", ["👕", "👖", "👗", "🧦", "🧢", "👟"]),
        ("thời tiết", "🌈", ["☀️", "🌧️", "⛄", "🌈", "⚡", "☁️"]),
        ("bánh kẹo", "🍭", ["🍰", "🍩", "🍪", "🍭", "🍫", "🧁"]),
        ("côn trùng", "🐞", ["🐝", "🐞", "🦋", "🐛", "🐜", "🐌"]),
        ("hoa", "🌸", ["🌸", "🌻", "🌹", "🌷", "🌼", "💐"]),
        ("nhạc cụ", "🎸", ["🎸", "🥁", "🎹", "🎺", "🎻", "🎷"]),
        ("thể thao", "⚽", ["⚽", "🏀", "🏈", "🎾", "🏐", "🏓"]),
    ];

    private static List<BaiHoc> TriNho()
    {
        var b = new Bo("trinho", 10);
        var r = new Random(606);
        for (var k = 0; k < ChuDeThe.Length; k++)
        {
            var (ten, icon, e) = ChuDeThe[k];
            var (tuoi, cap1, cap2) = k < 4 ? (3, 3, 3) : k < 8 ? (4, 4, 4) : (5, 5, 6);
            b.Them($"Lật thẻ {ten}", icon, tuoi,
            [
                Ghep($"Bé lật thẻ tìm các cặp {ten} giống nhau nhé!", LayNhieu(r, e, cap1)),
                Ghep("Thêm một lượt nữa nào!", LayNhieu(r, e, cap2)),
            ]);
        }
        return b.Ds;
    }

    // ---------- Chữ cái ----------
    private static readonly (string Hoa, string Thuong, string Doc)[] BangChu =
    [
        ("A", "a", "a"), ("Ă", "ă", "á"), ("Â", "â", "ớ"), ("B", "b", "bê"), ("C", "c", "xê"), ("D", "d", "dê"), ("Đ", "đ", "đê"),
        ("E", "e", "e"), ("Ê", "ê", "ê"), ("G", "g", "giê"), ("H", "h", "hát"), ("I", "i", "i"), ("K", "k", "ca"), ("L", "l", "e lờ"),
        ("M", "m", "em mờ"), ("N", "n", "en nờ"), ("O", "o", "o"), ("Ô", "ô", "ô"), ("Ơ", "ơ", "ơ"), ("P", "p", "pê"), ("Q", "q", "quy"),
        ("R", "r", "e rờ"), ("S", "s", "ét xì"), ("T", "t", "tê"), ("U", "u", "u"), ("Ư", "ư", "ư"), ("V", "v", "vê"), ("X", "x", "ích xì"), ("Y", "y", "i dài"),
    ];

    /// <summary>Từ bắt đầu bằng một chữ cái (tránh phụ âm ghép như th, ch, ng, nh, tr).</summary>
    private static readonly (string E, string Tu, string Chu)[] TuDau =
    [
        ("🐄", "bò", "B"), ("🐟", "cá", "C"), ("🐔", "gà", "G"), ("🐱", "mèo", "M"), ("🐘", "voi", "V"), ("🐝", "ong", "O"),
        ("🐸", "ếch", "Ê"), ("🐐", "dê", "D"), ("🐢", "rùa", "R"), ("🦁", "sư tử", "S"), ("🐯", "hổ", "H"), ("🦌", "nai", "N"),
        ("🍐", "lê", "L"), ("🥭", "xoài", "X"), ("🍑", "đào", "Đ"), ("🐌", "ốc", "Ô"), ("⭐", "sao", "S"), ("🌸", "hoa", "H"),
        ("🦆", "vịt", "V"), ("🐻", "gấu", "G"), ("🦀", "cua", "C"), ("🐍", "rắn", "R"), ("🍇", "nho", "N"), ("🚌", "xe buýt", "X"),
    ];

    private static List<BaiHoc> ChuCai()
    {
        var b = new Bo("chu", 1);
        var r = new Random(707);
        var cum = BangChu.Chunk(5).ToList();

        CauHoi TimChu(IReadOnlyList<(string Hoa, string Thuong, string Doc)> nhom)
        {
            var c = Lay(r, nhom);
            return Chon(r, $"Bé tìm chữ {c.Doc} nhé!", "", c.Hoa, nhom.Select(x => x.Hoa), 4);
        }
        CauHoi NoiHoaThuong(IReadOnlyList<(string Hoa, string Thuong, string Doc)> nhom) =>
            Noi("Bé nối chữ in hoa với chữ thường giống nó nhé!", LayNhieu(r, nhom, 4).Select(c => (c.Hoa, c.Thuong)));
        CauHoi ChuDau()
        {
            var t = Lay(r, TuDau);
            var doc = BangChu.First(c => c.Hoa == t.Chu).Doc;
            return Chon(r, $"Hình nào bắt đầu bằng chữ {doc}?", t.Chu, t.E, TuDau.Where(x => x.Chu != t.Chu).Select(x => x.E));
        }

        for (var k = 0; k < cum.Count; k++)
        {
            var nhom = cum[k];
            b.Them($"Làm quen chữ {nhom[0].Hoa} đến {nhom[^1].Hoa}", nhom[0].Hoa, k < 2 ? 4 : 5, () => TimChu(nhom));
        }
        for (var k = 0; k < cum.Count; k++)
        {
            var nhom = cum[k];
            b.Them($"Chữ hoa, chữ thường {nhom[0].Hoa}–{nhom[^1].Hoa}", nhom[0].Thuong, 5, () => NoiHoaThuong(nhom), 2);
        }
        for (var k = 1; k <= 4; k++) b.Them($"Chữ cái đầu tiên – bài {k}", "🔤", k <= 2 ? 5 : 6, ChuDau);
        for (var k = 1; k <= 2; k++) b.Them($"Ôn tập chữ cái – bài {k}", "🏅", 6, () => TimChu(BangChu));
        return b.Ds;
    }
}
