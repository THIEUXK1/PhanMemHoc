namespace PhanMemHoc.Models;

public class BaiHoc
{
    public int Id { get; set; }
    public string TieuDe { get; set; } = "";
    public string NangLuc { get; set; } = "";
    public string Icon { get; set; } = "⭐";
    public int TuoiTu { get; set; } = 3;
    public int TuoiDen { get; set; } = 6;
    /// <summary>Thứ tự trong nhóm năng lực; bài sau mở khoá khi bài trước đạt ≥ 1 sao.</summary>
    public int ThuTu { get; set; }
    public bool HienThi { get; set; } = true;
    public List<CauHoi> CauHoi { get; set; } = [];
}

public static class LoaiCauHoi
{
    /// <summary>Chọn 1 đáp án đúng trong các lựa chọn (emoji hoặc chữ).</summary>
    public const string Chon = "chon";
    /// <summary>Lật thẻ tìm các cặp giống nhau.</summary>
    public const string Ghep = "ghep";
    /// <summary>Nối đôi: mỗi phần tử Cap dạng "trái=phải", bé chạm trái rồi chạm phải để nối.</summary>
    public const string Noi = "noi";
    /// <summary>Xếp thứ tự: LuaChon theo đúng thứ tự, bé kéo/chạm lần lượt vào các ô.</summary>
    public const string SapXep = "sapxep";
}

public class CauHoi
{
    public string Loai { get; set; } = LoaiCauHoi.Chon;
    /// <summary>Câu hỏi, được đọc to cho bé nghe.</summary>
    public string DeBai { get; set; } = "";
    /// <summary>Hình minh hoạ hiển thị to phía trên (emoji/chữ), có thể để trống.</summary>
    public string Hien { get; set; } = "";
    public List<string> LuaChon { get; set; } = [];
    public int DapAn { get; set; }
    /// <summary>Loại "ghep": mỗi phần tử sinh ra 2 thẻ giống nhau. Loại "noi": mỗi phần tử "trái=phải".</summary>
    public List<string> Cap { get; set; } = [];
}
