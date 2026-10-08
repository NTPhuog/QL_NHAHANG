using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Models;

var builder = WebApplication.CreateBuilder(args);
if (builder.Configuration["DataProtection:KeyPath"] is string keyPath) builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(keyPath));

// MVC
builder.Services.AddControllersWithViews(options => { options.Filters.Add<RestaurantManagement.Security.AccessFilter>(); options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute()); });
builder.Services.AddDistributedMemoryCache();

// Database
builder.Services.AddDbContext<QuanLyNhaHangContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("QuanLyNhaHang")
    ));

// Session - PHẢI đặt trước builder.Build()
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();
if (args.Contains("--create-admin"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<QuanLyNhaHangContext>();
    if (await db.Admins.AnyAsync()) throw new InvalidOperationException("Đã có admin; chức năng này chỉ dùng để khởi tạo lần đầu.");
    var email = Environment.GetEnvironmentVariable("RESTAURANT_ADMIN_EMAIL")?.Trim().ToLowerInvariant();
    var password = Environment.GetEnvironmentVariable("RESTAURANT_ADMIN_PASSWORD");
    if (email == null || email.Length > 100 || !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email) || password == null || password.Length < 12 || password.Length > 128)
        throw new InvalidOperationException("Cần email hợp lệ và mật khẩu admin từ 12–128 ký tự qua biến môi trường RESTAURANT_ADMIN_EMAIL / RESTAURANT_ADMIN_PASSWORD.");
    db.Admins.Add(new Admin { HoTen = "Quản trị viên", Email = email, MatKhau = RestaurantManagement.Security.Passwords.Hash(password), TrangThai = "Hoạt động" });
    await db.SaveChangesAsync();
    Console.WriteLine("Đã tạo admin đầu tiên.");
    return;
}

// =========================
// Middleware
// =========================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

// Session phải được bật sau UseRouting
// và trước khi Controller sử dụng Session
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();


