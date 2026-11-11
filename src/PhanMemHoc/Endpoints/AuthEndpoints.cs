using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using PhanMemHoc.Data;
using PhanMemHoc.Models;

namespace PhanMemHoc.Endpoints;

public record DangKyDto(string HoTen, string Email, string MatKhau);
public record DangNhapDto(string Email, string MatKhau);

public static class AuthEndpoints
{
    public static int PhuHuynhId(this ClaimsPrincipal u) => int.Parse(u.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private static object ThongTin(PhuHuynh p) => new { p.Id, p.HoTen, p.Email, p.LaAdmin };

    private static Task DangNhapCookie(HttpContext ctx, PhuHuynh p)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, p.Id.ToString()), new(ClaimTypes.Name, p.HoTen) };
        if (p.LaAdmin) claims.Add(new(ClaimTypes.Role, "Admin"));
        var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id),
            new AuthenticationProperties { IsPersistent = true });
    }

    public static void MapAuth(this WebApplication app)
    {
        var g = app.MapGroup("/api/auth");

        g.MapPost("/dangky", async (DangKyDto dto, AppDb db, IPasswordHasher<PhuHuynh> hasher, HttpContext ctx) =>
        {
            var email = dto.Email?.Trim().ToLowerInvariant() ?? "";
            if (string.IsNullOrWhiteSpace(dto.HoTen) || !email.Contains('@'))
                return Results.BadRequest("Vui lòng nhập họ tên và email hợp lệ.");
            if ((dto.MatKhau ?? "").Length < 6) return Results.BadRequest("Mật khẩu cần ít nhất 6 ký tự.");
            if (db.PhuHuynh.Exists(x => x.Email == email)) return Results.Conflict("Email này đã được đăng ký.");

            var p = new PhuHuynh { HoTen = dto.HoTen.Trim(), Email = email };
            p.MatKhauHash = hasher.HashPassword(p, dto.MatKhau!);
            db.PhuHuynh.Insert(p);
            await DangNhapCookie(ctx, p);
            return Results.Ok(ThongTin(p));
        });

        g.MapPost("/dangnhap", async (DangNhapDto dto, AppDb db, IPasswordHasher<PhuHuynh> hasher, HttpContext ctx) =>
        {
            var email = dto.Email?.Trim().ToLowerInvariant() ?? "";
            var p = db.PhuHuynh.FindOne(x => x.Email == email);
            if (p is null || hasher.VerifyHashedPassword(p, p.MatKhauHash, dto.MatKhau ?? "") == PasswordVerificationResult.Failed)
                return Results.BadRequest("Email hoặc mật khẩu không đúng.");
            await DangNhapCookie(ctx, p);
            return Results.Ok(ThongTin(p));
        });

        g.MapPost("/dangxuat", async (HttpContext ctx) =>
        {
            await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.NoContent();
        });

        g.MapGet("/toi", (ClaimsPrincipal u, AppDb db) =>
            db.PhuHuynh.FindById(u.PhuHuynhId()) is { } p ? Results.Ok(ThongTin(p)) : Results.Unauthorized())
            .RequireAuthorization();
    }
}
