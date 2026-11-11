using LiteDB;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using PhanMemHoc.Data;
using PhanMemHoc.Endpoints;
using PhanMemHoc.Models;

var builder = WebApplication.CreateBuilder(args);

var duongDanDb = Path.Combine(builder.Environment.ContentRootPath, "phanmemhoc.db");
builder.Services.AddSingleton<ILiteDatabase>(_ => new LiteDatabase($"Filename={duongDanDb};Connection=direct"));
builder.Services.AddSingleton<AppDb>();
builder.Services.AddSingleton<IPasswordHasher<PhuHuynh>, PasswordHasher<PhuHuynh>>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.Cookie.Name = "phanmemhoc.auth";
        o.Cookie.HttpOnly = true;
        o.Cookie.SameSite = SameSiteMode.Strict;
        o.ExpireTimeSpan = TimeSpan.FromDays(30);
        o.SlidingExpiration = true;
        // API trả mã lỗi thay vì chuyển hướng sang trang đăng nhập
        o.Events.OnRedirectToLogin = ctx => { ctx.Response.StatusCode = 401; return Task.CompletedTask; };
        o.Events.OnRedirectToAccessDenied = ctx => { ctx.Response.StatusCode = 403; return Task.CompletedTask; };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

DuLieuMau.KhoiTao(app.Services.GetRequiredService<AppDb>(),
    app.Services.GetRequiredService<IPasswordHasher<PhuHuynh>>(), app.Logger);

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapAuth();
app.MapHoc();
app.MapAdmin();
app.MapTts();

app.Run();
