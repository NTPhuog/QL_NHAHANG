using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RestaurantManagement.Models;
using RestaurantManagement.Security;
namespace RestaurantManagement.Hosting;
public sealed record SiteIdentity(string Role);
public static class RestaurantHost
{
    public static WebApplication Create(string[] args, string role, int port)
    {
        if(role is not("KhachHang" or "NhanVien" or "Admin"))throw new ArgumentException("Unknown site role");
        var builder=WebApplication.CreateBuilder(args.Where(x=>x!="--create-admin").ToArray());
        if(string.IsNullOrWhiteSpace(builder.Configuration["urls"]))builder.WebHost.UseUrls($"http://localhost:{port}");
        builder.Services.AddSingleton(new SiteIdentity(role));
        builder.Services.AddScoped<RestaurantManagement.Services.BookingWorkflow>();
        builder.Services.AddScoped<RestaurantManagement.Services.BookingExpiry>();
        builder.Services.AddHostedService<RestaurantManagement.Services.BookingExpiryWorker>();
        var imageRoot=Path.GetFullPath(builder.Configuration["Images:Root"] ?? Path.Combine(builder.Environment.ContentRootPath,"..","SharedUploads"));
        Directory.CreateDirectory(Path.Combine(imageRoot,"mon-an")); Directory.CreateDirectory(Path.Combine(imageRoot,"ban-an"));
        builder.Services.AddSingleton(new ImageStorage(imageRoot));
        var protection=builder.Services.AddDataProtection().SetApplicationName("Restaurant."+role);
        if(builder.Configuration["DataProtection:KeyPath"] is string path)protection.PersistKeysToFileSystem(new DirectoryInfo(path));
        builder.Services.AddControllersWithViews(options=>{
            options.Filters.Add<SiteAccessFilter>(); options.Filters.Add<RestaurantManagement.Services.BookingExpiryFilter>();
            options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
        }).AddCookieTempDataProvider(options=>options.Cookie.Name=".Restaurant."+role+".TempData");
        builder.Services.AddAntiforgery(options=>options.Cookie.Name=".Restaurant."+role+".Antiforgery");
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession(options=>{
            options.Cookie.Name=".Restaurant."+role+".Session";
            options.Cookie.HttpOnly=true;
            options.Cookie.IsEssential=true;
            options.Cookie.SameSite=SameSiteMode.Lax;
            options.IdleTimeout=TimeSpan.FromMinutes(30);
        });
        builder.Services.AddDbContext<QuanLyNhaHangContext>(options=>options.UseSqlServer(builder.Configuration.GetConnectionString("QuanLyNhaHang")));
        var app=builder.Build();
        if(!app.Environment.IsDevelopment())app.UseExceptionHandler("/error");
        app.UseStaticFiles(new StaticFileOptions { FileProvider=new PhysicalFileProvider(imageRoot), RequestPath="/uploads", OnPrepareResponse=context=> { context.Context.Response.Headers["X-Content-Type-Options"]="nosniff"; context.Context.Response.Headers["Cache-Control"]="public,max-age=86400"; } });
        app.UseRouting();app.UseSession();app.UseAuthorization();
        app.MapGet("/error",()=>Results.Problem("Không thể xử lý yêu cầu. Vui lòng thử lại.",statusCode:500));
        if(role!="KhachHang")app.MapGet("/",()=>Results.Redirect("/"+role));
        app.MapStaticAssets();
        if(role!="KhachHang")app.MapControllerRoute("areas","{area:exists}/{controller=Home}/{action=Index}/{id?}");
        app.MapControllerRoute("default","{controller=Home}/{action=Index}/{id?}").WithStaticAssets();
        return app;
    }
    public static async Task<bool> InitializeAsync(WebApplication app,string[] args,string role)
    {
        if(!args.Contains("--create-admin"))return false;
        if(role!="Admin")throw new InvalidOperationException("Chỉ ứng dụng Admin được khởi tạo quản trị viên.");
        using var scope=app.Services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<QuanLyNhaHangContext>();
        if(await db.Admins.AnyAsync())throw new InvalidOperationException("Đã có tài khoản admin; không cần khởi tạo lại.");
        var email=Environment.GetEnvironmentVariable("RESTAURANT_ADMIN_EMAIL")?.Trim().ToLowerInvariant();
        var password=Environment.GetEnvironmentVariable("RESTAURANT_ADMIN_PASSWORD");
        if(email==null||email.Length>100||!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email)||password==null||password.Length is <12 or >128)throw new InvalidOperationException("Cần email hợp lệ và mật khẩu 12–128 ký tự qua RESTAURANT_ADMIN_EMAIL và RESTAURANT_ADMIN_PASSWORD.");
        db.Admins.Add(new Admin{HoTen="Quản trị viên",Email=email,MatKhau=Passwords.Hash(password),TrangThai="Hoạt động"});
        await db.SaveChangesAsync();Console.WriteLine("Đã khởi tạo admin.");return true;
    }
}

