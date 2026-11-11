namespace PhanMemHoc.Models;

public class HoSoBe
{
    public int Id { get; set; }
    public int PhuHuynhId { get; set; }
    public string Ten { get; set; } = "";
    public int NamSinh { get; set; }
    public string AnhDaiDien { get; set; } = "🐻";
    public DateTime NgayTao { get; set; } = DateTime.Now;

    public int Tuoi(DateTime homNay) => Math.Max(0, homNay.Year - NamSinh);
}
