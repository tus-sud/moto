using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MotoShopWeb.Data;
using MotoShopWeb.Models;
using MotoShopWeb.Security;
using MotoShopWeb.ViewModels;

namespace MotoShopWeb.Controllers;

[Authorize(Policy = "StaffReady")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class WorkspaceController(ApplicationDbContext db, VehicleAccess access,
    ILogger<WorkspaceController> logger) : Controller
{
    private int ActorId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private async Task<bool> Commit(Func<Task<int>> write, string action, string table, string detail)
    {
        await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            var id = await write();
            db.NhatKyHeThongs.Add(new NhatKyHeThong
            {
                MaNv = ActorId, HanhDong = action, BangTacDong = table, MaBanGhi = id,
                ChiTiet = detail.Length > 1000 ? detail[..1000] : detail, ThoiGian = DateTime.Now
            });
            await db.SaveChangesAsync();
            await tx.CommitAsync();
            TempData["Success"] = "Đã lưu thay đổi.";
            return true;
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            db.ChangeTracker.Clear();
            logger.LogError(ex, "Không lưu được {Action} trên {Table}", action, table);
            ModelState.AddModelError("", "Không lưu được: dữ liệu có thể bị trùng, vượt giới hạn hoặc vi phạm ràng buộc. Hãy kiểm tra thông tin và thử lại.");
            return false;
        }
    }

    private static async Task<PageVm<T>> Paged<T>(IQueryable<T> query, string? q, int page)
    {
        const int size = 15;
        int count = await query.CountAsync();
        int pages = Math.Max(1, (int)Math.Ceiling(count / (double)size));
        page = Math.Clamp(page, 1, pages);
        return new PageVm<T>
        {
            Items = await query.Skip((page - 1) * size).Take(size).ToListAsync(),
            Q = q, Total = count, Page = page, TotalPages = pages
        };
    }

    public async Task<IActionResult> Index()
    {
        if (User.IsInRole("Admin")) return RedirectToAction("Index", "Admin");
        var permissions = await access.GetAsync(User);
        return View(new WorkspaceHomeVm
        {
            Permissions = permissions,
            RoleTitle = await db.NhanViens.Where(n => n.MaNv == ActorId)
                .Select(n => n.MaChucVuNavigation.TenChucVu).SingleAsync(),
            VehicleCount = permissions.Contains(VehiclePermissions.View) ? await db.Xes.CountAsync() : null
        });
    }

    [Authorize(Policy = VehiclePermissions.View)]
    public async Task<IActionResult> VehicleDetails(int id)
    {
        var xe = await db.Xes.AsNoTracking().Include(x => x.MaHangNavigation)
            .Include(x => x.MaLoaiNavigation).Include(x => x.AnhXes).Include(x => x.MauXes)
            .SingleOrDefaultAsync(x => x.MaXe == id);
        if (xe is null) return NotFound();
        return View(new VehicleDetailVm { Vehicle = xe, Permissions = await access.GetAsync(User) });
    }

    [Authorize(Policy = VehiclePermissions.View)]
    public async Task<IActionResult> Vehicles(string? q, int page = 1)
    {
        q = q?.Trim();
        var query = db.Xes.AsNoTracking().Include(x => x.MaHangNavigation).Include(x => x.MaLoaiNavigation).AsQueryable();
        if (!string.IsNullOrEmpty(q)) query = query.Where(x => x.TenXe.Contains(q) || x.MaHangNavigation.TenHang.Contains(q));
        return View(new WorkspaceVehiclesVm
        {
            Results = await Paged(query.OrderByDescending(x => x.MaXe), q, page),
            Permissions = await access.GetAsync(User)
        });
    }

    private async Task LoadVehicle(XeForm form)
    {
        ViewBag.CanChangeVisibility = await access.CanAsync(User, VehiclePermissions.Visibility);
        form.Hangs = await db.HangXes.OrderBy(x => x.TenHang)
            .Select(x => new SelectListItem(x.TenHang, x.MaHang.ToString())).ToListAsync();
        form.Loais = await db.LoaiXes.OrderBy(x => x.TenLoai)
            .Select(x => new SelectListItem(x.TenLoai, x.MaLoai.ToString())).ToListAsync();
        form.MauHienCo = string.Join(", ", await db.MauXes.Where(x => x.MaXe == form.Id).Select(x => x.TenMau).ToListAsync());
    }

    [HttpGet, Authorize(Policy = VehiclePermissions.View)]
    public async Task<IActionResult> VehicleEdit(int id = 0)
    {
        if (!await access.CanAsync(User, id == 0 ? VehiclePermissions.Create : VehiclePermissions.Edit)) return Forbid();
        var form = new XeForm();
        if (id != 0)
        {
            var x = await db.Xes.AsNoTracking().SingleOrDefaultAsync(x => x.MaXe == id);
            if (x is null) return NotFound();
            form = new XeForm
            {
                Id = x.MaXe, TenXe = x.TenXe, MaHang = x.MaHang, MaLoai = x.MaLoai,
                NamSanXuat = x.NamSanXuat, GiaNiemYet = x.GiaNiemYet, DungTichCc = x.DungTichCc,
                CongSuatKw = x.CongSuatKw, HopSo = x.HopSo, KieuDanDong = x.KieuDanDong,
                SoCho = x.SoCho, MoTa = x.MoTa, HienThi = x.HienThi,
                AnhChinh = await db.AnhXes.Where(a => a.MaXe == id && a.LaAnhChinh).Select(a => a.DuongDan).FirstOrDefaultAsync()
            };
        }
        await LoadVehicle(form);
        return View(form);
    }

    private static bool ValidImage(string? path) => string.IsNullOrWhiteSpace(path)
        || (path.StartsWith("/images/", StringComparison.Ordinal) && !path.Contains('\\') && !path.Contains(".."))
        || (Uri.TryCreate(path, UriKind.Absolute, out var url) && url.Scheme == "https" && string.IsNullOrEmpty(url.UserInfo));

    [HttpPost, Authorize(Policy = VehiclePermissions.View)]
    public async Task<IActionResult> VehicleEdit(XeForm form)
    {
        if (!await access.CanAsync(User, form.Id == 0 ? VehiclePermissions.Create : VehiclePermissions.Edit)) return Forbid();
        bool canChangeVisibility = await access.CanAsync(User, VehiclePermissions.Visibility);
        form.TenXe = form.TenXe?.Trim() ?? "";
        form.AnhChinh = form.AnhChinh?.Trim();
        if (!await db.HangXes.AnyAsync(h => h.MaHang == form.MaHang)) ModelState.AddModelError(nameof(form.MaHang), "Chọn hãng xe hợp lệ.");
        if (!await db.LoaiXes.AnyAsync(l => l.MaLoai == form.MaLoai)) ModelState.AddModelError(nameof(form.MaLoai), "Chọn loại xe hợp lệ.");
        if (!ValidImage(form.AnhChinh)) ModelState.AddModelError(nameof(form.AnhChinh), "Dùng URL https:// hoặc đường dẫn /images/ten-anh.jpg.");
        var colors = (form.MauMoi ?? "").Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (colors.Any(c => c.Length > 50)) ModelState.AddModelError(nameof(form.MauMoi), "Mỗi tên màu tối đa 50 ký tự.");
        if (!ModelState.IsValid) { await LoadVehicle(form); return View(form); }
        Xe? xe = form.Id == 0 ? new Xe() : await db.Xes.FindAsync(form.Id);
        if (xe is null) return NotFound();
        bool isNew = form.Id == 0;
        xe.TenXe = form.TenXe; xe.MaHang = form.MaHang; xe.MaLoai = form.MaLoai;
        xe.NamSanXuat = form.NamSanXuat; xe.GiaNiemYet = form.GiaNiemYet; xe.DungTichCc = form.DungTichCc;
        xe.CongSuatKw = form.CongSuatKw; xe.HopSo = form.HopSo; xe.KieuDanDong = form.KieuDanDong;
        xe.SoCho = form.SoCho; xe.MoTa = form.MoTa; xe.HienThi = canChangeVisibility ? form.HienThi : (isNew ? false : xe.HienThi);
        if (isNew) db.Xes.Add(xe);
        if (await Commit(async () =>
        {
            await db.SaveChangesAsync();
            var photo = await db.AnhXes.SingleOrDefaultAsync(a => a.MaXe == xe.MaXe && a.LaAnhChinh);
            if (string.IsNullOrWhiteSpace(form.AnhChinh))
            {
                if (photo is not null) db.AnhXes.Remove(photo);
            }
            else if (photo is null) db.AnhXes.Add(new AnhXe { MaXe = xe.MaXe, DuongDan = form.AnhChinh, LaAnhChinh = true });
            else photo.DuongDan = form.AnhChinh;
            foreach (var color in colors)
                if (!await db.MauXes.AnyAsync(m => m.MaXe == xe.MaXe && m.TenMau == color))
                    db.MauXes.Add(new MauXe { MaXe = xe.MaXe, TenMau = color });
            await db.SaveChangesAsync();
            return xe.MaXe;
        }, isNew ? "THEM_XE" : "SUA_XE", "Xe", "Mẫu xe: " + form.TenXe)) return RedirectToAction(nameof(Vehicles));
        await LoadVehicle(form);
        return View(form);
    }

    [HttpPost, Authorize(Policy = VehiclePermissions.Visibility)]
    public async Task<IActionResult> VehicleVisibility(int id, bool visible)
    {
        if (!ModelState.IsValid) return BadRequest();
        var xe = await db.Xes.FindAsync(id);
        if (xe is null) return NotFound();
        xe.HienThi = visible;
        if (!await Commit(async () => { await db.SaveChangesAsync(); return id; },
            visible ? "HIEN_XE" : "AN_XE", "Xe", xe.TenXe)) TempData["Error"] = "Không đổi được trạng thái xe.";
        return RedirectToAction(nameof(Vehicles));
    }

}
