using System.Security.Claims;
using PhanMemHoc.Data;
using PhanMemHoc.Models;
using PhanMemHoc.Services;

namespace PhanMemHoc.Endpoints;

public record BeDto(string Ten, int NamSinh, string? AnhDaiDien);
public record NopBaiDto(int BaiHocId, List<int> TraLoi, int ThoiGianGiay);

public static class HocEndpoints
{
    private static string? KiemTraBe(BeDto dto, int namNay)
    {
        if (string.IsNullOrWhiteSpace(dto.Ten) || dto.Ten.Trim().Length > 40) return "Tên bé cần 1–40 ký tự.";
        if (dto.NamSinh < namNay - 10 || dto.NamSinh > namNay - 1) return "Năm sinh không hợp lệ.";
        return null;
    }

    public static void MapHoc(this WebApplication app)
    {
        app.MapGet("/api/nangluc", () => DanhMucNangLuc.TatCa);

        var g = app.MapGroup("/api/be").RequireAuthorization();

        g.MapGet("/", (ClaimsPrincipal u, AppDb db) =>
        {
            var ds = db.Be.Find(x => x.PhuHuynhId == u.PhuHuynhId()).ToList();
            var tuoiHomNay = DateTime.Now;
            return ds.Select(b => new
            {
                b.Id, b.Ten, b.NamSinh, b.AnhDaiDien, Tuoi = b.Tuoi(tuoiHomNay),
                TongSao = db.KetQuaCuaBe(b.Id).GroupBy(k => k.BaiHocId).Sum(x => x.Max(k => k.Sao)),
            });
        });

        g.MapPost("/", (BeDto dto, ClaimsPrincipal u, AppDb db) =>
        {
            if (KiemTraBe(dto, DateTime.Now.Year) is { } loi) return Results.BadRequest(loi);
            var be = new HoSoBe
            {
                PhuHuynhId = u.PhuHuynhId(), Ten = dto.Ten.Trim(), NamSinh = dto.NamSinh,
                AnhDaiDien = string.IsNullOrWhiteSpace(dto.AnhDaiDien) ? "🐻" : dto.AnhDaiDien.Trim(),
            };
            db.Be.Insert(be);
            return Results.Ok(be);
        });

        g.MapPut("/{id:int}", (int id, BeDto dto, ClaimsPrincipal u, AppDb db) =>
        {
            var be = db.LayBeCuaPhuHuynh(id, u.PhuHuynhId());
            if (be is null) return Results.NotFound();
            if (KiemTraBe(dto, DateTime.Now.Year) is { } loi) return Results.BadRequest(loi);
            be.Ten = dto.Ten.Trim();
            be.NamSinh = dto.NamSinh;
            if (!string.IsNullOrWhiteSpace(dto.AnhDaiDien)) be.AnhDaiDien = dto.AnhDaiDien.Trim();
            db.Be.Update(be);
            return Results.Ok(be);
        });

        g.MapDelete("/{id:int}", (int id, ClaimsPrincipal u, AppDb db) =>
        {
            if (db.LayBeCuaPhuHuynh(id, u.PhuHuynhId()) is null) return Results.NotFound();
            db.XoaBe(id);
            return Results.NoContent();
        });

        g.MapGet("/{id:int}/lotrinh", (int id, ClaimsPrincipal u, AppDb db) =>
        {
            var be = db.LayBeCuaPhuHuynh(id, u.PhuHuynhId());
            if (be is null) return Results.NotFound();
            var tuoi = be.Tuoi(DateTime.Now);
            var lt = LoTrinh.Tao(db.BaiHoc.FindAll(), db.KetQuaCuaBe(id), tuoi);
            return Results.Ok(new { Be = new { be.Id, be.Ten, be.AnhDaiDien, Tuoi = tuoi }, lt.Nhom, lt.GoiYBaiId, lt.TongSao, HuyHieu = LoTrinh.TinhHuyHieu(lt) });
        });

        g.MapGet("/{id:int}/baihoc/{baiId:int}", (int id, int baiId, ClaimsPrincipal u, AppDb db) =>
        {
            var be = db.LayBeCuaPhuHuynh(id, u.PhuHuynhId());
            if (be is null) return Results.NotFound();
            var lt = LoTrinh.Tao(db.BaiHoc.FindAll(), db.KetQuaCuaBe(id), be.Tuoi(DateTime.Now));
            if (!lt.Nhom.SelectMany(n => n.Bai).Any(b => b.Id == baiId && b.DaMo))
                return Results.BadRequest("Bài học này chưa mở khoá.");
            return Results.Ok(db.BaiHoc.FindById(baiId));
        });

        g.MapPost("/{id:int}/ketqua", (int id, NopBaiDto dto, ClaimsPrincipal u, AppDb db) =>
        {
            var be = db.LayBeCuaPhuHuynh(id, u.PhuHuynhId());
            if (be is null) return Results.NotFound();
            var bai = db.BaiHoc.FindById(dto.BaiHocId);
            if (bai is null) return Results.NotFound();
            if (dto.TraLoi is null || dto.TraLoi.Count != bai.CauHoi.Count) return Results.BadRequest("Số câu trả lời không khớp.");

            var tuoi = be.Tuoi(DateTime.Now);
            var truoc = LoTrinh.Tao(db.BaiHoc.FindAll(), db.KetQuaCuaBe(id), tuoi);

            var dung = LoTrinh.ChamDiem(bai, dto.TraLoi);
            var kq = new KetQua
            {
                BeId = id, BaiHocId = bai.Id, NangLuc = bai.NangLuc, SoCauDung = dung, TongCau = bai.CauHoi.Count,
                Sao = LoTrinh.TinhSao(dung, bai.CauHoi.Count), ThoiGianGiay = Math.Clamp(dto.ThoiGianGiay, 0, 3600),
            };
            db.KetQua.Insert(kq);

            var sau = LoTrinh.Tao(db.BaiHoc.FindAll(), db.KetQuaCuaBe(id), tuoi);
            var daMoTruoc = truoc.Nhom.SelectMany(n => n.Bai).Where(b => b.DaMo).Select(b => b.Id).ToHashSet();
            var moKhoaMoi = sau.Nhom.SelectMany(n => n.Bai).Where(b => b.DaMo && !daMoTruoc.Contains(b.Id))
                .Select(b => new { b.Id, b.TieuDe, b.Icon }).ToList();

            var coTruoc = LoTrinh.TinhHuyHieu(truoc).Where(h => h.DaCo).Select(h => h.Ma).ToHashSet();
            var huyHieuMoi = LoTrinh.TinhHuyHieu(sau).Where(h => h.DaCo && !coTruoc.Contains(h.Ma)).ToList();

            return Results.Ok(new { kq.SoCauDung, kq.TongCau, kq.Sao, MoKhoaMoi = moKhoaMoi, HuyHieuMoi = huyHieuMoi, sau.GoiYBaiId });
        });

        g.MapGet("/{id:int}/baocao", (int id, ClaimsPrincipal u, AppDb db) =>
        {
            var be = db.LayBeCuaPhuHuynh(id, u.PhuHuynhId());
            if (be is null) return Results.NotFound();
            var tuoi = be.Tuoi(DateTime.Now);
            var bc = LoTrinh.TaoBaoCao(db.BaiHoc.FindAll(), db.KetQuaCuaBe(id), tuoi, DateTime.Now);
            return Results.Ok(new { Be = new { be.Id, be.Ten, be.AnhDaiDien, Tuoi = tuoi }, BaoCao = bc });
        });
    }
}
