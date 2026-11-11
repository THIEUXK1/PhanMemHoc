namespace PhanMemHoc.Models;

/// <summary>Một lượt học xong một bài.</summary>
public class KetQua
{
    public int Id { get; set; }
    public int BeId { get; set; }
    public int BaiHocId { get; set; }
    public string NangLuc { get; set; } = "";
    public int SoCauDung { get; set; }
    public int TongCau { get; set; }
    public int Sao { get; set; }
    public int ThoiGianGiay { get; set; }
    public DateTime NgayHoc { get; set; } = DateTime.Now;
}
