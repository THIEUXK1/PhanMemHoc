namespace PhanMemHoc.Models;

public class PhuHuynh
{
    public int Id { get; set; }
    public string HoTen { get; set; } = "";
    public string Email { get; set; } = "";
    public string MatKhauHash { get; set; } = "";
    public bool LaAdmin { get; set; }
    public DateTime NgayTao { get; set; } = DateTime.Now;
}
