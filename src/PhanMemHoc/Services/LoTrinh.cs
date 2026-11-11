using PhanMemHoc.Models;

namespace PhanMemHoc.Services;

public record BaiTrongLoTrinh(int Id, string TieuDe, string Icon, int ThuTu, int SoCau, int SaoCaoNhat, bool DaMo);
public record NhomLoTrinh(string Ma, string Ten, string Icon, List<BaiTrongLoTrinh> Bai);
public record KetQuaLoTrinh(List<NhomLoTrinh> Nhom, int? GoiYBaiId, int TongSao);

public record BaoCaoNangLuc(string Ma, string Ten, string Icon, string MoTa, string HoatDongThucTe,
    int SoLuot, int TiLeDung, int SoBaiXong, int TongBai);
public record LuotGanDay(string TieuDe, string Icon, int Sao, int SoCauDung, int TongCau, DateTime NgayHoc);
public record PhutTheoNgay(DateOnly Ngay, int Phut);
public record BaoCao(int TongLuot, int TongSao, int TongPhut, int SoNgayHoc7, List<BaoCaoNangLuc> NangLuc,
    List<PhutTheoNgay> BayNgay, List<LuotGanDay> GanDay, string? NenLuyenThem);

/// <summary>Logic thuần (không truy cập DB) cho chấm điểm, mở khoá bài, gợi ý và báo cáo.</summary>
public static class LoTrinh
{
    public const int TuoiNhoNhat = 3, TuoiLonNhat = 6;

    public static int TinhSao(int dung, int tong)
    {
        if (tong <= 0) return 0;
        var tiLe = (double)dung / tong;
        return tiLe >= 1 ? 3 : tiLe >= 0.7 ? 2 : tiLe >= 0.4 ? 1 : 0;
    }

    /// <summary>
    /// Chấm theo lần trả lời đầu tiên của bé. Câu "chon": traLoi = chỉ số đáp án đã chọn.
    /// Câu "ghep": traLoi = số lần lật sai; đúng nếu sai không quá số cặp.
    /// </summary>
    public static int ChamDiem(BaiHoc bai, IReadOnlyList<int> traLoi)
    {
        var dung = 0;
        for (var i = 0; i < bai.CauHoi.Count && i < traLoi.Count; i++)
        {
            var c = bai.CauHoi[i];
            if (c.Loai == LoaiCauHoi.Ghep ? traLoi[i] >= 0 && traLoi[i] <= c.Cap.Count : traLoi[i] == c.DapAn)
                dung++;
        }
        return dung;
    }

    public static bool HopTuoi(BaiHoc b, int tuoi)
    {
        var t = Math.Clamp(tuoi, TuoiNhoNhat, TuoiLonNhat);
        return b.TuoiTu <= t && t <= b.TuoiDen;
    }

    public static KetQuaLoTrinh Tao(IEnumerable<BaiHoc> tatCaBai, IReadOnlyCollection<KetQua> ketQua, int tuoi)
    {
        var saoCaoNhat = ketQua.GroupBy(k => k.BaiHocId).ToDictionary(g => g.Key, g => g.Max(k => k.Sao));
        var baiHopLe = tatCaBai.Where(b => b.HienThi && HopTuoi(b, tuoi)).ToList();

        var nhom = new List<NhomLoTrinh>();
        foreach (var nl in DanhMucNangLuc.TatCa)
        {
            var ds = new List<BaiTrongLoTrinh>();
            var baiTruocDat = true;
            foreach (var b in baiHopLe.Where(b => b.NangLuc == nl.Ma).OrderBy(b => b.ThuTu).ThenBy(b => b.Id))
            {
                var sao = saoCaoNhat.GetValueOrDefault(b.Id);
                ds.Add(new(b.Id, b.TieuDe, b.Icon, b.ThuTu, b.CauHoi.Count, sao, baiTruocDat));
                baiTruocDat = baiTruocDat && sao >= 1;
            }
            if (ds.Count > 0) nhom.Add(new(nl.Ma, nl.Ten, nl.Icon, ds));
        }

        return new(nhom, ChonGoiY(nhom, ketQua), saoCaoNhat.Values.Sum());
    }

    /// <summary>
    /// Ưu tiên bài chưa học ở nhóm năng lực bé đang yếu nhất (tỉ lệ đúng thấp nhất, rồi ít luyện nhất);
    /// hết bài mới thì gợi ý ôn bài đã mở có ít sao nhất.
    /// </summary>
    private static int? ChonGoiY(List<NhomLoTrinh> nhom, IReadOnlyCollection<KetQua> ketQua)
    {
        double DiemNhom(string ma)
        {
            var kq = ketQua.Where(k => k.NangLuc == ma).ToList();
            var tong = kq.Sum(k => k.TongCau);
            return tong == 0 ? -1 : (double)kq.Sum(k => k.SoCauDung) / tong; // chưa học → ưu tiên trước
        }

        var baiMoi = nhom
            .Select((n, i) => (n, i, bai: n.Bai.FirstOrDefault(b => b.DaMo && b.SaoCaoNhat == 0)))
            .Where(x => x.bai is not null)
            .OrderBy(x => DiemNhom(x.n.Ma)).ThenBy(x => ketQua.Count(k => k.NangLuc == x.n.Ma)).ThenBy(x => x.i)
            .Select(x => x.bai)
            .FirstOrDefault();
        if (baiMoi is not null) return baiMoi.Id;

        return nhom.SelectMany(n => n.Bai).Where(b => b.DaMo && b.SaoCaoNhat < 3)
            .OrderBy(b => b.SaoCaoNhat).Select(b => (int?)b.Id).FirstOrDefault();
    }

    public static BaoCao TaoBaoCao(IEnumerable<BaiHoc> tatCaBai, IReadOnlyCollection<KetQua> ketQua, int tuoi, DateTime bayGio)
    {
        var bai = tatCaBai.ToDictionary(b => b.Id);
        var saoCaoNhat = ketQua.GroupBy(k => k.BaiHocId).ToDictionary(g => g.Key, g => g.Max(k => k.Sao));
        var baiHopLe = bai.Values.Where(b => b.HienThi && HopTuoi(b, tuoi)).ToList();

        var nangLuc = DanhMucNangLuc.TatCa.Select(nl =>
        {
            var kq = ketQua.Where(k => k.NangLuc == nl.Ma).ToList();
            var tongCau = kq.Sum(k => k.TongCau);
            var baiNhom = baiHopLe.Where(b => b.NangLuc == nl.Ma).ToList();
            return new BaoCaoNangLuc(nl.Ma, nl.Ten, nl.Icon, nl.MoTa, nl.HoatDongThucTe,
                kq.Count,
                tongCau == 0 ? 0 : (int)Math.Round(100.0 * kq.Sum(k => k.SoCauDung) / tongCau),
                baiNhom.Count(b => saoCaoNhat.GetValueOrDefault(b.Id) >= 1),
                baiNhom.Count);
        }).ToList();

        var homNay = DateOnly.FromDateTime(bayGio);
        var bayNgay = Enumerable.Range(0, 7).Select(i => homNay.AddDays(i - 6))
            .Select(ngay => new PhutTheoNgay(ngay, (int)Math.Ceiling(ketQua
                .Where(k => DateOnly.FromDateTime(k.NgayHoc) == ngay).Sum(k => k.ThoiGianGiay) / 60.0)))
            .ToList();

        var ganDay = ketQua.OrderByDescending(k => k.NgayHoc).Take(10)
            .Select(k => bai.TryGetValue(k.BaiHocId, out var b)
                ? new LuotGanDay(b.TieuDe, b.Icon, k.Sao, k.SoCauDung, k.TongCau, k.NgayHoc)
                : new LuotGanDay("(bài đã xoá)", "❔", k.Sao, k.SoCauDung, k.TongCau, k.NgayHoc))
            .ToList();

        var yeuNhat = nangLuc.Where(n => n.SoLuot > 0 && n.TiLeDung < 80).OrderBy(n => n.TiLeDung).FirstOrDefault();

        return new BaoCao(
            ketQua.Count,
            saoCaoNhat.Values.Sum(),
            (int)Math.Ceiling(ketQua.Sum(k => k.ThoiGianGiay) / 60.0),
            bayNgay.Count(n => ketQua.Any(k => DateOnly.FromDateTime(k.NgayHoc) == n.Ngay)),
            nangLuc, bayNgay, ganDay, yeuNhat?.Ma);
    }

    /// <summary>Kiểm tra bài học do admin nhập. Trả về thông báo lỗi hoặc null nếu hợp lệ.</summary>
    public static string? KiemTra(BaiHoc b)
    {
        if (string.IsNullOrWhiteSpace(b.TieuDe)) return "Tiêu đề không được để trống.";
        if (DanhMucNangLuc.Tim(b.NangLuc) is null) return "Nhóm năng lực không hợp lệ.";
        if (b.TuoiTu < 2 || b.TuoiDen > 8 || b.TuoiTu > b.TuoiDen) return "Độ tuổi phải trong khoảng 2–8 và 'từ' ≤ 'đến'.";
        if (b.CauHoi.Count == 0) return "Bài học cần ít nhất 1 câu hỏi.";
        for (var i = 0; i < b.CauHoi.Count; i++)
        {
            var c = b.CauHoi[i];
            var so = i + 1;
            if (string.IsNullOrWhiteSpace(c.DeBai)) return $"Câu {so}: chưa nhập đề bài.";
            switch (c.Loai)
            {
                case LoaiCauHoi.Chon:
                    if (c.LuaChon.Count < 2 || c.LuaChon.Any(string.IsNullOrWhiteSpace))
                        return $"Câu {so}: cần ít nhất 2 lựa chọn, không để trống.";
                    if (c.DapAn < 0 || c.DapAn >= c.LuaChon.Count) return $"Câu {so}: đáp án đúng không hợp lệ.";
                    break;
                case LoaiCauHoi.Ghep:
                    if (c.Cap.Count < 2 || c.Cap.Count > 8 || c.Cap.Any(string.IsNullOrWhiteSpace))
                        return $"Câu {so}: trò lật thẻ cần 2–8 hình.";
                    if (c.Cap.Distinct().Count() != c.Cap.Count) return $"Câu {so}: các hình lật thẻ phải khác nhau.";
                    break;
                default:
                    return $"Câu {so}: loại câu hỏi không hợp lệ.";
            }
        }
        return null;
    }
}
