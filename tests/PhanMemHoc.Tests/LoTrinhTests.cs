using LiteDB;
using PhanMemHoc.Data;
using PhanMemHoc.Models;
using PhanMemHoc.Services;

namespace PhanMemHoc.Tests;

public class LoTrinhTests
{
    private static BaiHoc Bai(int id, string nl, int thuTu, int tuoiTu = 3, int soCau = 4) => new()
    {
        Id = id, NangLuc = nl, ThuTu = thuTu, TieuDe = $"Bài {id}", TuoiTu = tuoiTu, TuoiDen = 6,
        CauHoi = Enumerable.Range(0, soCau).Select(_ => new CauHoi { DeBai = "?", LuaChon = ["a", "b"], DapAn = 1 }).ToList(),
    };

    private static KetQua Kq(int baiId, string nl, int dung, int tong = 4) =>
        new() { BaiHocId = baiId, NangLuc = nl, SoCauDung = dung, TongCau = tong, Sao = LoTrinh.TinhSao(dung, tong) };

    [Theory]
    [InlineData(4, 4, 3)]
    [InlineData(3, 4, 2)]
    [InlineData(2, 4, 1)]
    [InlineData(1, 4, 0)]
    [InlineData(0, 0, 0)]
    public void TinhSao_TheoTiLeDung(int dung, int tong, int sao) => Assert.Equal(sao, LoTrinh.TinhSao(dung, tong));

    [Fact]
    public void ChamDiem_CauChonVaCauGhep()
    {
        var bai = new BaiHoc
        {
            CauHoi =
            [
                new() { Loai = LoaiCauHoi.Chon, LuaChon = ["a", "b", "c"], DapAn = 2 },
                new() { Loai = LoaiCauHoi.Chon, LuaChon = ["a", "b"], DapAn = 0 },
                new() { Loai = LoaiCauHoi.Ghep, Cap = ["x", "y", "z"] },
                new() { Loai = LoaiCauHoi.Ghep, Cap = ["x", "y"] },
            ],
        };
        // đúng, sai, ghép sai 3 lần (= số cặp, vẫn đạt), ghép sai 5 lần (> số cặp, không đạt)
        Assert.Equal(2, LoTrinh.ChamDiem(bai, [2, 1, 3, 5]));
    }

    [Fact]
    public void Tao_ChiMoBaiDauMoiNhom_KhiChuaHoc()
    {
        var lt = LoTrinh.Tao([Bai(1, "so", 1), Bai(2, "so", 2), Bai(3, "hinh", 1)], [], 4);

        var so = lt.Nhom.Single(n => n.Ma == "so").Bai;
        Assert.True(so[0].DaMo);
        Assert.False(so[1].DaMo);
        Assert.True(lt.Nhom.Single(n => n.Ma == "hinh").Bai[0].DaMo);
    }

    [Fact]
    public void Tao_MoBaiTiepTheo_KhiBaiTruocCoItNhat1Sao()
    {
        var bai = new[] { Bai(1, "so", 1), Bai(2, "so", 2), Bai(3, "so", 3) };

        var chuaDat = LoTrinh.Tao(bai, [Kq(1, "so", 1)], 4).Nhom[0].Bai;
        Assert.False(chuaDat[1].DaMo);

        var dat = LoTrinh.Tao(bai, [Kq(1, "so", 1), Kq(1, "so", 2)], 4).Nhom[0].Bai;
        Assert.Equal(1, dat[0].SaoCaoNhat);
        Assert.True(dat[1].DaMo);
        Assert.False(dat[2].DaMo);
    }

    [Fact]
    public void Tao_LocTheoTuoi_VaGioiHanTuoiNgoaiKhoang()
    {
        var bai = new[] { Bai(1, "so", 1, tuoiTu: 3), Bai(2, "so", 2, tuoiTu: 5) };

        Assert.Single(LoTrinh.Tao(bai, [], 3).Nhom[0].Bai);
        Assert.Single(LoTrinh.Tao(bai, [], 2).Nhom[0].Bai);   // bé 2 tuổi xem như 3 tuổi
        Assert.Equal(2, LoTrinh.Tao(bai, [], 9).Nhom[0].Bai.Count); // bé 9 tuổi xem như 6 tuổi
    }

    [Fact]
    public void Tao_GoiYNhomChuaHoc_TruocNhomDaHoc()
    {
        var bai = new[] { Bai(1, "so", 1), Bai(2, "so", 2), Bai(3, "hinh", 1) };
        var lt = LoTrinh.Tao(bai, [Kq(1, "so", 4)], 4);
        Assert.Equal(3, lt.GoiYBaiId);
    }

    [Fact]
    public void Tao_GoiYNhomYeuHon_KhiCaHaiDaHoc()
    {
        var bai = new[] { Bai(1, "so", 1), Bai(2, "so", 2), Bai(3, "hinh", 1), Bai(4, "hinh", 2) };
        // nhóm "so" đúng 75%, nhóm "hinh" đúng 50% → gợi ý bài tiếp của "hinh"
        var lt = LoTrinh.Tao(bai, [Kq(1, "so", 3), Kq(3, "hinh", 2)], 4);
        Assert.Equal(4, lt.GoiYBaiId);
    }

    [Fact]
    public void Tao_HetBaiMoi_GoiYOnBaiItSaoNhat()
    {
        var bai = new[] { Bai(1, "so", 1), Bai(2, "hinh", 1) };
        var lt = LoTrinh.Tao(bai, [Kq(1, "so", 4), Kq(2, "hinh", 2)], 4);
        Assert.Equal(2, lt.GoiYBaiId);
        Assert.Equal(4, lt.TongSao);
    }

    [Fact]
    public void TaoBaoCao_TinhTiLeTheoNangLuc_VaPhutBayNgay()
    {
        var bayGio = new DateTime(2026, 11, 11, 10, 0, 0);
        var kq = new List<KetQua>
        {
            new() { BaiHocId = 1, NangLuc = "so", SoCauDung = 3, TongCau = 4, Sao = 2, ThoiGianGiay = 90, NgayHoc = bayGio },
            new() { BaiHocId = 1, NangLuc = "so", SoCauDung = 1, TongCau = 4, Sao = 0, ThoiGianGiay = 30, NgayHoc = bayGio.AddDays(-2) },
            new() { BaiHocId = 1, NangLuc = "so", SoCauDung = 4, TongCau = 4, Sao = 3, ThoiGianGiay = 60, NgayHoc = bayGio.AddDays(-30) },
        };

        var bc = LoTrinh.TaoBaoCao([Bai(1, "so", 1), Bai(2, "so", 2)], kq, 4, bayGio);

        var so = bc.NangLuc.Single(n => n.Ma == "so");
        Assert.Equal(3, so.SoLuot);
        Assert.Equal(67, so.TiLeDung);          // 8/12
        Assert.Equal(1, so.SoBaiXong);
        Assert.Equal(2, so.TongBai);
        Assert.Equal(3, bc.TongSao);            // sao cao nhất của bài 1
        Assert.Equal(7, bc.BayNgay.Count);
        Assert.Equal(2, bc.BayNgay[^1].Phut);   // 90 giây → làm tròn lên 2 phút
        Assert.Equal(2, bc.SoNgayHoc7);
        Assert.Equal("so", bc.NenLuyenThem);
    }

    [Fact]
    public void KiemTra_BaoLoi_BaiHocKhongHopLe()
    {
        Assert.Null(LoTrinh.KiemTra(Bai(1, "so", 1)));
        Assert.NotNull(LoTrinh.KiemTra(Bai(1, "khongco", 1)));
        Assert.NotNull(LoTrinh.KiemTra(new BaiHoc { TieuDe = "x", NangLuc = "so" }));

        var dapAnSai = Bai(1, "so", 1);
        dapAnSai.CauHoi[0].DapAn = 5;
        Assert.NotNull(LoTrinh.KiemTra(dapAnSai));

        var ghepTrung = Bai(1, "trinho", 1);
        ghepTrung.CauHoi[0] = new CauHoi { Loai = LoaiCauHoi.Ghep, DeBai = "?", Cap = ["🐶", "🐶"] };
        Assert.NotNull(LoTrinh.KiemTra(ghepTrung));
    }

    [Fact]
    public void BaiHocMau_DeuHopLe() =>
        Assert.All(DuLieuMau.BaiHocMau(), b => Assert.Null(LoTrinh.KiemTra(b)));
}

public class AppDbTests
{
    private static AppDb TaoDb() => new(new LiteDatabase(new MemoryStream()));

    [Fact]
    public void LayBeCuaPhuHuynh_KhongTraBeCuaNguoiKhac()
    {
        var db = TaoDb();
        var id = db.Be.Insert(new HoSoBe { PhuHuynhId = 1, Ten = "Bin", NamSinh = 2022 }).AsInt32;

        Assert.NotNull(db.LayBeCuaPhuHuynh(id, 1));
        Assert.Null(db.LayBeCuaPhuHuynh(id, 2));
    }

    [Fact]
    public void XoaBe_XoaLuonKetQua()
    {
        var db = TaoDb();
        var id = db.Be.Insert(new HoSoBe { PhuHuynhId = 1, Ten = "Bin", NamSinh = 2022 }).AsInt32;
        db.KetQua.Insert(new KetQua { BeId = id, BaiHocId = 1 });
        db.KetQua.Insert(new KetQua { BeId = 999, BaiHocId = 1 });

        db.XoaBe(id);

        Assert.Null(db.Be.FindById(id));
        Assert.Empty(db.KetQuaCuaBe(id));
        Assert.Single(db.KetQuaCuaBe(999));
    }

    [Fact]
    public void Email_PhuHuynh_KhongTrung()
    {
        var db = TaoDb();
        db.PhuHuynh.Insert(new PhuHuynh { Email = "a@x.com" });
        Assert.Throws<LiteException>(() => db.PhuHuynh.Insert(new PhuHuynh { Email = "a@x.com" }));
    }
}
