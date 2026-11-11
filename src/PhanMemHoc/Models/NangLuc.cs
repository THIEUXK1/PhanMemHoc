namespace PhanMemHoc.Models;

/// <summary>Nhóm năng lực tư duy — danh mục cố định, bài học gắn vào một nhóm.</summary>
public record NangLuc(string Ma, string Ten, string Icon, string MoTa, string HoatDongThucTe);

public static class DanhMucNangLuc
{
    public static readonly IReadOnlyList<NangLuc> TatCa =
    [
        new("so", "Số và đếm", "🔢", "Đếm số lượng, nhận biết chữ số, thứ tự số.",
            "Cùng bé đếm số bậc cầu thang, số cúc áo, số quả trong đĩa khi ăn."),
        new("hinh", "Hình và màu", "🔷", "Nhận biết hình dạng và màu sắc cơ bản.",
            "Đi dạo và cùng bé tìm đồ vật hình tròn, hình vuông, đồ vật màu đỏ quanh nhà."),
        new("sosanh", "So sánh", "⚖️", "To – nhỏ, nhiều – ít, lớn hơn – bé hơn.",
            "Cho bé so sánh hai cốc nước, hai đôi dép của bố và của bé xem cái nào to hơn."),
        new("quyluat", "Quy luật", "🔁", "Nhận ra quy luật lặp lại và tìm phần tiếp theo.",
            "Xếp đồ chơi theo quy luật (xanh – đỏ – xanh – đỏ) rồi hỏi bé món tiếp theo là gì."),
        new("phanloai", "Phân loại", "🧺", "Tìm vật khác loại, xếp đồ vật theo nhóm.",
            "Nhờ bé phân loại quần áo khi gấp đồ, hoặc xếp rau củ và trái cây riêng ra."),
        new("trinho", "Trí nhớ", "🧠", "Ghi nhớ hình ảnh, lật thẻ tìm cặp giống nhau.",
            "Đặt 3–4 đồ vật lên bàn, cho bé nhìn rồi che lại và hỏi vừa có những gì."),
        new("chu", "Chữ cái", "🔤", "Nhận biết chữ cái tiếng Việt, chữ hoa – chữ thường, chữ cái đầu của từ.",
            "Cùng bé tìm chữ cái trên bao bì, biển hiệu; đố bé \"con gì bắt đầu bằng chữ B?\"."),
    ];

    public static NangLuc? Tim(string ma) => TatCa.FirstOrDefault(x => x.Ma == ma);
}
