using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotoShopWeb.Data;
using MotoShopWeb.Models;
using MotoShopWeb.ViewModels;

namespace MotoShopWeb.Controllers;

[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class WishlistController(ApplicationDbContext db) : Controller
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
            return RedirectToAction("Login", "Customer", new { returnUrl = "/Wishlist" });

        await SchemaInitializer.EnsureCompatibleSchemaAsync(db);
        var items = await db.YeuThiches.AsNoTracking()
            .Where(y => y.MaKh == customerId.Value && y.MaXeNavigation.HienThi)
            .OrderByDescending(y => y.NgayThem)
            .Select(y => new StoreCard
            {
                Id = y.MaXe,
                Name = y.MaXeNavigation.TenXe,
                Brand = y.MaXeNavigation.MaHangNavigation.TenHang,
                Category = y.MaXeNavigation.MaLoaiNavigation.TenLoai,
                Price = y.MaXeNavigation.GiaNiemYet,
                Engine = y.MaXeNavigation.DungTichCc,
                Year = y.MaXeNavigation.NamSanXuat,
                Image = y.MaXeNavigation.AnhXes
                    .OrderByDescending(a => a.LaAnhChinh).ThenBy(a => a.ThuTu)
                    .Select(a => a.DuongDan).FirstOrDefault() ?? "/images/store-placeholder.svg"
            }).ToListAsync();

        return View(items);
    }

    [HttpGet]
    public IActionResult Toggle(int? bikeId)
    {
        if (bikeId.HasValue) return RedirectToAction("Details", "Home", new { id = bikeId.Value });
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Toggle(int bikeId, string? returnUrl = null)
    {
        var customerId = await GetCustomerIdAsync();
        if (customerId is null)
        {
            TempData["Error"] = "Vui lòng đăng nhập để lưu mẫu xe yêu thích.";
            return RedirectToAction("Login", "Customer", new { returnUrl = Url.Action("Details", "Home", new { id = bikeId }) });
        }

        await SchemaInitializer.EnsureCompatibleSchemaAsync(db);
        var existing = await db.YeuThiches.SingleOrDefaultAsync(y => y.MaKh == customerId.Value && y.MaXe == bikeId);
        if (existing is null)
        {
            if (await db.Xes.AnyAsync(x => x.MaXe == bikeId && x.HienThi))
            {
                db.YeuThiches.Add(new YeuThich { MaKh = customerId.Value, MaXe = bikeId, NgayThem = DateTime.Now });
                await db.SaveChangesAsync();
                TempData["Success"] = "Đã lưu mẫu xe vào danh sách Yêu thích ♥";
            }
        }
        else
        {
            db.YeuThiches.Remove(existing);
            await db.SaveChangesAsync();
            TempData["Success"] = "Đã xóa mẫu xe khỏi danh sách Yêu thích.";
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);
        return RedirectToAction("Details", "Home", new { id = bikeId });
    }

    [HttpPost]
    public async Task<IActionResult> AddReview(int bikeId, int rating, string? comment)
    {
        var customerId = await GetCustomerIdAsync();
        if (customerId is null)
        {
            TempData["Error"] = "Vui lòng đăng nhập để gửi đánh giá mẫu xe.";
            return RedirectToAction("Login", "Customer", new { returnUrl = Url.Action("Details", "Home", new { id = bikeId }) });
        }

        await SchemaInitializer.EnsureCompatibleSchemaAsync(db);
        rating = Math.Clamp(rating, 1, 5);
        comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        if (comment?.Length > 1000) comment = comment[..1000];

        var existing = await db.DanhGia.SingleOrDefaultAsync(d => d.MaKh == customerId.Value && d.MaXe == bikeId);
        if (existing is null)
        {
            db.DanhGia.Add(new DanhGia
            {
                MaKh = customerId.Value,
                MaXe = bikeId,
                SoSao = rating,
                NoiDung = comment,
                NgayDanhGia = DateTime.Now
            });
            TempData["Success"] = "Cảm ơn bạn đã gửi đánh giá cho mẫu xe này!";
        }
        else
        {
            existing.SoSao = rating;
            existing.NoiDung = comment;
            existing.NgayDanhGia = DateTime.Now;
            TempData["Success"] = "Đã cập nhật đánh giá của bạn.";
        }

        await db.SaveChangesAsync();
        return RedirectToAction("Details", "Home", new { id = bikeId });
    }
}
