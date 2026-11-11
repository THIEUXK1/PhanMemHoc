using LiteDB;
using PhanMemHoc.Models;

namespace PhanMemHoc.Data;

/// <summary>Gom các collection LiteDB của ứng dụng. LiteDatabase thread-safe nên dùng singleton.</summary>
public class AppDb
{
    public ILiteDatabase Db { get; }
    public ILiteCollection<PhuHuynh> PhuHuynh { get; }
    public ILiteCollection<HoSoBe> Be { get; }
    public ILiteCollection<BaiHoc> BaiHoc { get; }
    public ILiteCollection<KetQua> KetQua { get; }

    public AppDb(ILiteDatabase db)
    {
        Db = db;
        PhuHuynh = db.GetCollection<PhuHuynh>("phu_huynh");
        Be = db.GetCollection<HoSoBe>("ho_so_be");
        BaiHoc = db.GetCollection<BaiHoc>("bai_hoc");
        KetQua = db.GetCollection<KetQua>("ket_qua");

        PhuHuynh.EnsureIndex(x => x.Email, unique: true);
        Be.EnsureIndex(x => x.PhuHuynhId);
        KetQua.EnsureIndex(x => x.BeId);
        KetQua.EnsureIndex(x => x.BaiHocId);
    }

    /// <summary>Trả về hồ sơ bé nếu thuộc phụ huynh này, ngược lại null.</summary>
    public HoSoBe? LayBeCuaPhuHuynh(int beId, int phuHuynhId)
    {
        var be = Be.FindById(beId);
        return be?.PhuHuynhId == phuHuynhId ? be : null;
    }

    public List<KetQua> KetQuaCuaBe(int beId) => KetQua.Find(x => x.BeId == beId).ToList();

    public void XoaBe(int beId)
    {
        KetQua.DeleteMany(x => x.BeId == beId);
        Be.Delete(beId);
    }
}
