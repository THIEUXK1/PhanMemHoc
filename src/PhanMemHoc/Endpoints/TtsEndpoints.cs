using System.Security.Cryptography;
using System.Text;

namespace PhanMemHoc.Endpoints;

/// Giọng đọc dự phòng khi trình duyệt không có giọng vi-VN: lấy mp3 tiếng Việt từ Google Translate TTS, cache ra đĩa.
public static class TtsEndpoints
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(10) };

    public static void MapTts(this WebApplication app)
    {
        var thuMuc = Path.Combine(app.Environment.ContentRootPath, "tts-cache");
        Directory.CreateDirectory(thuMuc);

        app.MapGet("/api/tts", async (string? q, CancellationToken ct) =>
        {
            q = q?.Trim();
            if (string.IsNullOrEmpty(q) || q.Length > 200) return Results.BadRequest("Câu đọc cần 1–200 ký tự.");

            var ten = Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(q))) + ".mp3";
            var duongDan = Path.Combine(thuMuc, ten);
            if (!File.Exists(duongDan))
            {
                var url = "https://translate.google.com/translate_tts?ie=UTF-8&client=tw-ob&tl=vi&q=" + Uri.EscapeDataString(q);
                using var req = new HttpRequestMessage(HttpMethod.Get, url);
                req.Headers.UserAgent.ParseAdd("Mozilla/5.0");
                using var res = await Http.SendAsync(req, ct);
                if (!res.IsSuccessStatusCode) return Results.StatusCode(502);
                var bytes = await res.Content.ReadAsByteArrayAsync(ct);
                var tam = duongDan + ".tmp";
                await File.WriteAllBytesAsync(tam, bytes, ct);
                File.Move(tam, duongDan, overwrite: true);
            }
            return Results.File(duongDan, "audio/mpeg", enableRangeProcessing: true);
        }).RequireAuthorization();
    }
}
