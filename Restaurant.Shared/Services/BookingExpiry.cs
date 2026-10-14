using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RestaurantManagement.Models;

namespace RestaurantManagement.Services;

public sealed class BookingExpiry(QuanLyNhaHangContext db)
{
    public async Task<int> ExpireAsync(CancellationToken cancellationToken = default)
    {
        var cutoff = BookingRules.Now.AddHours(-2);
        var day = DateOnly.FromDateTime(cutoff);
        var time = TimeOnly.FromDateTime(cutoff);
        // A single conditional UPDATE makes concurrent workers and check-in safe.
        return await db.DatBans.Where(d =>
            (d.TrangThai == "Chờ xác nhận" || d.TrangThai == "Đã duyệt") &&
            d.CheckInUtc == null && d.MaHoaDon == null &&
            (d.NgayDat < day || (d.NgayDat == day && d.GioDat <= time)))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(d => d.TrangThai, "Đã hủy")
                .SetProperty(d => d.LyDoHuy, "Tự động hủy: quá hạn 2 giờ chưa check-in.")
                .SetProperty(d => d.CocDaHoan, d => d.TienCoc > 0), cancellationToken);
    }
}

public sealed class BookingExpiryWorker(IServiceScopeFactory scopes, ILogger<BookingExpiryWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        do
        {
            try
            {
                using var scope = scopes.CreateScope();
                await scope.ServiceProvider.GetRequiredService<BookingExpiry>().ExpireAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
            catch (Exception ex) { logger.LogError(ex, "Unable to expire overdue reservations; will retry."); }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}

public sealed class BookingExpiryFilter(BookingExpiry expiry) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.RouteData.Values["controller"]?.ToString() is "DatBan" or "BanAn")
            await expiry.ExpireAsync(context.HttpContext.RequestAborted);
        await next();
    }
}
