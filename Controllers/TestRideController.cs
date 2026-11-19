using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotoShopWeb.Data;
using MotoShopWeb.Models;

namespace MotoShopWeb.Controllers;

[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class TestRideController(ApplicationDbContext db) : Controller
{
    private async Task<int?> GetCustomerIdAsync()
    {
        var result = await HttpContext.AuthenticateAsync("Customer");
        if (!result.Succeeded) return null;
        var val = result.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(val, out var id) ? id : null;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var customerId = await GetCustomerIdAsync();
        if (customerId is null)
            return RedirectToAction("Login", "Customer", new { returnUrl = "/TestRide" });

        await SchemaInitializer.EnsureCompatibleSchemaAsync(db);
        var list = await db.LichLaiThus.AsNoTracking()
            .Include(l => l.MaXeNavigation).ThenInclude(x => x.MaHangNavigation)
            .Where(l => l.MaKh == customerId.Value)
            .OrderByDescending(l => l.NgayHen)
            .ToListAsync();
        return View(list);
    }

    [HttpPost]
    public async Task<IActionResult> Book(int bikeId, DateTime appointmentDate, string? note)
    {
        var customerId = await GetCustomerIdAsync();
        if (customerId is null)
        {
            TempData["Error"] = "Vui lòng đăng nhập để đặt lịch hẹn lái thử xe.";
            return RedirectToAction("Login", "Customer", new { returnUrl = Url.Action("Details", "Home", new { id = bikeId }) });
        }

        await SchemaInitializer.EnsureCompatibleSchemaAsync(db);
        if (appointmentDate <= DateTime.Now.AddHours(1))
        {
            TempData["Error"] = "Thời gian hẹn lái thử cần cách hiện tại ít nhất 1 giờ.";
            return RedirectToAction("Details", "Home", new { id = bikeId });
        }

        if (!await db.Xes.AnyAsync(x => x.MaXe == bikeId && x.HienThi))
            return NotFound();

        note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        if (note?.Length > 500) note = note[..500];

        db.LichLaiThus.Add(new LichLaiThu
        {
            MaKh = customerId.Value,
            MaXe = bikeId,
            NgayHen = appointmentDate,
            TrangThai = "ChoXacNhan",
            GhiChu = note
        });
        await db.SaveChangesAsync();
        TempData["Success"] = "Đã gửi yêu cầu đặt lịch lái thử! Cửa hàng sẽ liên hệ xác nhận với bạn.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Cancel(int id)
    {
        var customerId = await GetCustomerIdAsync();
        if (customerId is null)
            return RedirectToAction("Login", "Customer", new { returnUrl = "/TestRide" });

        await SchemaInitializer.EnsureCompatibleSchemaAsync(db);
        var item = await db.LichLaiThus.SingleOrDefaultAsync(l => l.MaLich == id && l.MaKh == customerId.Value);
        if (item is not null && item.TrangThai == "ChoXacNhan")
        {
            item.TrangThai = "DaHuy";
            await db.SaveChangesAsync();
            TempData["Success"] = "Đã hủy lịch hẹn lái thử.";
        }
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "AdminOnly"), HttpGet]
    public async Task<IActionResult> Admin()
    {
        await SchemaInitializer.EnsureCompatibleSchemaAsync(db);
        var list = await db.LichLaiThus.AsNoTracking()
            .Include(l => l.MaKhNavigation)
            .Include(l => l.MaXeNavigation)
            .OrderByDescending(l => l.MaLich)
            .Take(100)
            .ToListAsync();
        return View(list);
    }

    [Authorize(Policy = "AdminOnly"), HttpPost]
    public async Task<IActionResult> AdminUpdate(int id, string status)
    {
        if (status is not ("DaXacNhan" or "HoanThanh" or "DaHuy")) return BadRequest();
        await SchemaInitializer.EnsureCompatibleSchemaAsync(db);
        var item = await db.LichLaiThus.FindAsync(id);
        if (item is not null)
        {
            item.TrangThai = status;
            await db.SaveChangesAsync();
            TempData["Success"] = "Đã cập nhật trạng thái lịch lái thử #" + id;
        }
        return RedirectToAction(nameof(Admin));
    }
}
