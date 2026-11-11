using PhanMemHoc.Data;
using PhanMemHoc.Models;
using PhanMemHoc.Services;

namespace PhanMemHoc.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdmin(this WebApplication app)
    {
        var g = app.MapGroup("/api/admin").RequireAuthorization(p => p.RequireRole("Admin"));

        g.MapGet("/baihoc", (AppDb db) => db.BaiHoc.FindAll()
            .OrderBy(b => DanhMucNangLuc.TatCa.ToList().FindIndex(n => n.Ma == b.NangLuc)).ThenBy(b => b.ThuTu)
            .Select(b => new
            {
                b.Id, b.TieuDe, b.Icon, b.NangLuc, b.ThuTu, b.TuoiTu, b.TuoiDen, b.HienThi, SoCau = b.CauHoi.Count,
                SoLuot = db.KetQua.Count(k => k.BaiHocId == b.Id),
            }));

        g.MapGet("/baihoc/{id:int}", (int id, AppDb db) =>
            db.BaiHoc.FindById(id) is { } b ? Results.Ok(b) : Results.NotFound());

        g.MapPost("/baihoc", (BaiHoc b, AppDb db) =>
        {
            ChuanHoa(b);
            if (LoTrinh.KiemTra(b) is { } loi) return Results.BadRequest(loi);
            b.Id = 0;
            db.BaiHoc.Insert(b);
            return Results.Ok(b);
        });

        g.MapPut("/baihoc/{id:int}", (int id, BaiHoc b, AppDb db) =>
        {
            if (db.BaiHoc.FindById(id) is null) return Results.NotFound();
            ChuanHoa(b);
            if (LoTrinh.KiemTra(b) is { } loi) return Results.BadRequest(loi);
            b.Id = id;
            db.BaiHoc.Update(b);
            return Results.Ok(b);
        });

        g.MapDelete("/baihoc/{id:int}", (int id, AppDb db) =>
        {
            if (!db.BaiHoc.Delete(id)) return Results.NotFound();
            db.KetQua.DeleteMany(k => k.BaiHocId == id);
            return Results.NoContent();
        });

        g.MapGet("/thongke", (AppDb db) => new
        {
            PhuHuynh = db.PhuHuynh.Count(x => !x.LaAdmin),
            Be = db.Be.Count(),
            BaiHoc = db.BaiHoc.Count(),
            LuotHoc = db.KetQua.Count(),
        });
    }

    private static void ChuanHoa(BaiHoc b)
    {
        b.TieuDe = b.TieuDe?.Trim() ?? "";
        b.Icon = string.IsNullOrWhiteSpace(b.Icon) ? "⭐" : b.Icon.Trim();
        b.CauHoi ??= [];
        foreach (var c in b.CauHoi)
        {
            c.DeBai = c.DeBai?.Trim() ?? "";
            c.Hien = c.Hien?.Trim() ?? "";
            c.LuaChon = (c.LuaChon ?? []).Select(x => x?.Trim() ?? "").ToList();
            c.Cap = (c.Cap ?? []).Select(x => x?.Trim() ?? "").Where(x => x != "").ToList();
            if (c.Loai == LoaiCauHoi.Ghep) c.LuaChon = [];
            else c.Cap = [];
        }
    }
}
