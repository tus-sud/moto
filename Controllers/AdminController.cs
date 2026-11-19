using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MotoShopWeb.Data;
using MotoShopWeb.Models;
using MotoShopWeb.Security;
using MotoShopWeb.ViewModels;

namespace MotoShopWeb.Controllers;

[Authorize(Policy = "AdminOnly")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class AdminController(ApplicationDbContext db, ILogger<AdminController> logger) : Controller
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
            ModelState.AddModelError("", "Không lưu được: dữ liệu có thể bị trùng hoặc vi phạm ràng buộc. Không được khóa/hạ quyền quản lý cuối cùng.");
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

    public async Task<IActionResult> Index() => View(new DashboardVm
    {
        Xe = await db.Xes.CountAsync(), NhanVien = await db.NhanViens.CountAsync(),
        KhachHang = await db.KhachHangs.CountAsync(),
        DonChoXuLy = await db.DonHangs.CountAsync(d => d.TrangThai == "ChoXacNhan"),
        Recent = await db.NhatKyHeThongs.AsNoTracking().Include(n => n.MaNvNavigation)
            .OrderByDescending(n => n.MaNk).Take(10).ToListAsync()
    });

    public async Task<IActionResult> Vehicles(string? q, int page = 1)
    {
        q = q?.Trim();
        var query = db.Xes.AsNoTracking().Include(x => x.MaHangNavigation).Include(x => x.MaLoaiNavigation).AsQueryable();
        if (!string.IsNullOrEmpty(q)) query = query.Where(x => x.TenXe.Contains(q) || x.MaHangNavigation.TenHang.Contains(q));
        return View(await Paged(query.OrderByDescending(x => x.MaXe), q, page));
    }

    private async Task LoadVehicle(XeForm form)
    {
        form.Hangs = await db.HangXes.OrderBy(x => x.TenHang)
            .Select(x => new SelectListItem(x.TenHang, x.MaHang.ToString())).ToListAsync();
        form.Loais = await db.LoaiXes.OrderBy(x => x.TenLoai)
            .Select(x => new SelectListItem(x.TenLoai, x.MaLoai.ToString())).ToListAsync();
        form.MauHienCo = string.Join(", ", await db.MauXes.Where(x => x.MaXe == form.Id).Select(x => x.TenMau).ToListAsync());
    }

    [HttpGet]
    public async Task<IActionResult> VehicleEdit(int id = 0)
    {
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

    [HttpPost]
    public async Task<IActionResult> VehicleEdit(XeForm form)
    {
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
        xe.SoCho = form.SoCho; xe.MoTa = form.MoTa; xe.HienThi = form.HienThi;
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

    [HttpPost]
    public async Task<IActionResult> VehicleVisibility(int id, bool visible)
    {
        var xe = await db.Xes.FindAsync(id);
        if (xe is null) return NotFound();
        xe.HienThi = visible;
        if (!await Commit(async () => { await db.SaveChangesAsync(); return id; },
            visible ? "HIEN_XE" : "AN_XE", "Xe", xe.TenXe)) TempData["Error"] = "Không đổi được trạng thái xe.";
        return RedirectToAction(nameof(Vehicles));
    }

    public async Task<IActionResult> Catalog()
    {
        ViewBag.Brands = await db.HangXes.AsNoTracking().OrderBy(h => h.TenHang).ToListAsync();
        ViewBag.Types = await db.LoaiXes.AsNoTracking().OrderBy(l => l.TenLoai).ToListAsync();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AddCatalog(string kind, string? name)
    {
        name = name?.Trim();
        if (string.IsNullOrEmpty(name) || name.Length > 100 || (kind != "brand" && kind != "type"))
        { TempData["Error"] = "Nhập tên hợp lệ, tối đa 100 ký tự."; return RedirectToAction(nameof(Catalog)); }
        if (await Commit(async () =>
        {
            if (kind == "brand")
            {
                var brand = new HangXe { TenHang = name };
                db.HangXes.Add(brand); await db.SaveChangesAsync(); return brand.MaHang;
            }
            var type = new LoaiXe { TenLoai = name };
            db.LoaiXes.Add(type); await db.SaveChangesAsync(); return type.MaLoai;
        }, "THEM_DANH_MUC", kind == "brand" ? "HangXe" : "LoaiXe", name)) return RedirectToAction(nameof(Catalog));
        TempData["Error"] = "Tên đã có hoặc không lưu được danh mục.";
        return RedirectToAction(nameof(Catalog));
    }

    public async Task<IActionResult> Employees(string? q, int page = 1)
    {
        q = q?.Trim();
        var query = db.NhanViens.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(q)) query = query.Where(n => n.HoTen.Contains(q) || n.Email.Contains(q)
            || (n.TaiKhoanNhanVienMaNvNavigation != null && n.TaiKhoanNhanVienMaNvNavigation.TenDangNhap.Contains(q)));
        return View(await Paged(query.OrderByDescending(n => n.MaNv).Select(n => new StaffRow
        {
            Id = n.MaNv, Name = n.HoTen, Email = n.Email, Role = n.MaChucVuNavigation.TenChucVu,
            IsAdmin = n.MaChucVuNavigation.ToanQuyen,
            Username = n.TaiKhoanNhanVienMaNvNavigation == null ? null : n.TaiKhoanNhanVienMaNvNavigation.TenDangNhap,
            Active = n.TrangThai == "DangLam" && n.TaiKhoanNhanVienMaNvNavigation != null && n.TaiKhoanNhanVienMaNvNavigation.TrangThai
        }), q, page));
    }

    private async Task LoadStaff(StaffForm form)
    {
        form.ChucVus = await db.ChucVus.OrderByDescending(c => c.ToanQuyen).ThenBy(c => c.TenChucVu)
            .Select(c => new SelectListItem(c.TenChucVu + (c.ToanQuyen ? " — TOÀN QUYỀN" : ""), c.MaChucVu.ToString())).ToListAsync();
        form.Quyens = await db.Quyens.AsNoTracking().Where(p => p.KyHieu != "QUAN_LY_XE").OrderBy(q => q.NhomChucNang).ThenBy(q => q.MaQuyen).ToListAsync();
        form.IsSelf = form.Id == ActorId;
    }

    [HttpGet]
    public async Task<IActionResult> EmployeeEdit(int id = 0)
    {
        var form = new StaffForm();
        if (id != 0)
        {
            var n = await db.NhanViens.AsNoTracking().Include(n => n.TaiKhoanNhanVienMaNvNavigation).SingleOrDefaultAsync(n => n.MaNv == id);
            if (n is null) return NotFound();
            var a = n.TaiKhoanNhanVienMaNvNavigation;
            form = new StaffForm
            {
                Id = id, HoTen = n.HoTen, Email = n.Email, SoDienThoai = n.SoDienThoai, DiaChi = n.DiaChi,
                MaChucVu = n.MaChucVu, TenDangNhap = a?.TenDangNhap ?? "", Active = a?.TrangThai ?? true,
                DangLam = n.TrangThai == "DangLam",
                QuyenIds = await db.NhanVienQuyens.Where(p => p.MaNv == id).Select(p => p.MaQuyen).ToListAsync()
            };
        }
        await LoadStaff(form);
        return View(form);
    }

    [HttpPost]
    public async Task<IActionResult> EmployeeEdit(StaffForm form)
    {
        form.HoTen = form.HoTen?.Trim() ?? ""; form.Email = form.Email?.Trim() ?? "";
        form.TenDangNhap = form.TenDangNhap?.Trim() ?? "";
        if (!await db.ChucVus.AnyAsync(c => c.MaChucVu == form.MaChucVu)) ModelState.AddModelError(nameof(form.MaChucVu), "Chức vụ không tồn tại.");
        if (await db.NhanViens.AnyAsync(n => n.Email == form.Email && n.MaNv != form.Id)) ModelState.AddModelError(nameof(form.Email), "Email đã được dùng.");
        if (await db.TaiKhoanNhanViens.AnyAsync(a => a.TenDangNhap == form.TenDangNhap && a.MaNv != form.Id))
            ModelState.AddModelError(nameof(form.TenDangNhap), "Tên đăng nhập đã được dùng.");
        var n = form.Id == 0 ? new NhanVien { HeSoLuong = 1, NgayVaoLam = DateOnly.FromDateTime(DateTime.Today), MaQuanLy = ActorId }
            : await db.NhanViens.Include(n => n.TaiKhoanNhanVienMaNvNavigation).SingleOrDefaultAsync(n => n.MaNv == form.Id);
        if (n is null) return NotFound();
        var account = n.TaiKhoanNhanVienMaNvNavigation;
        if ((account is null || !string.IsNullOrEmpty(form.TemporaryPassword)) && !StaffIdentity.StrongPassword(form.TemporaryPassword))
            ModelState.AddModelError(nameof(form.TemporaryPassword), "Mật khẩu tạm cần 12–128 ký tự, có chữ hoa, chữ thường và số.");
        if (form.Id == ActorId && (form.MaChucVu != n.MaChucVu || !form.Active || !form.DangLam || !string.IsNullOrEmpty(form.TemporaryPassword)))
            ModelState.AddModelError("", "Không tự khóa/hạ quyền hoặc đặt mật khẩu tạm cho tài khoản đang dùng. Muốn đổi mật khẩu, dùng mục Đổi mật khẩu.");
        var ids = form.QuyenIds.Distinct().ToList();
        var selectedCodes = await db.Quyens.Where(p => ids.Contains(p.MaQuyen))
            .Select(p => p.KyHieu).ToListAsync();
        if (selectedCodes.Any(code => code == VehiclePermissions.Create || code == VehiclePermissions.Edit
            || code == VehiclePermissions.Visibility) && !selectedCodes.Contains(VehiclePermissions.View))
            ModelState.AddModelError("", "Khi cấp Thêm/Sửa/Ẩn hiện xe, cần tích thêm quyền Xem xe.");
        if (await db.Quyens.CountAsync(p => ids.Contains(p.MaQuyen)) != ids.Count) ModelState.AddModelError("", "Danh sách quyền không hợp lệ.");
        if (!ModelState.IsValid) { await LoadStaff(form); return View(form); }
        bool isNew = form.Id == 0;
        if (isNew) db.NhanViens.Add(n);
        n.HoTen = form.HoTen; n.Email = form.Email; n.SoDienThoai = form.SoDienThoai; n.DiaChi = form.DiaChi;
        n.MaChucVu = form.MaChucVu; n.TrangThai = form.DangLam ? "DangLam" : "NghiViec";
        if (await Commit(async () =>
        {
            await db.SaveChangesAsync();
            if (account is null)
            {
                account = new TaiKhoanNhanVien { MaNv = n.MaNv, MaNguoiCap = ActorId, MatKhauHash = "" };
                db.TaiKhoanNhanViens.Add(account);
            }
            account.TenDangNhap = form.TenDangNhap; account.TrangThai = form.Active;
            if (!string.IsNullOrEmpty(form.TemporaryPassword))
            {
                account.MatKhauHash = new PasswordHasher<TaiKhoanNhanVien>().HashPassword(account, form.TemporaryPassword);
                account.BatBuocDoiMatKhau = true;
                account.NgayDoiMatKhau = DateTime.Now;
            }
            var existing = await db.NhanVienQuyens.Where(p => p.MaNv == n.MaNv).ToListAsync();
            db.NhanVienQuyens.RemoveRange(existing.Where(p => !ids.Contains(p.MaQuyen)));
            foreach (int id in ids.Where(id => existing.All(p => p.MaQuyen != id)))
                db.NhanVienQuyens.Add(new NhanVienQuyen { MaNv = n.MaNv, MaQuyen = id, MaNguoiCap = ActorId, NgayCap = DateTime.Now });
            await db.SaveChangesAsync();
            return n.MaNv;
        }, isNew ? "TAO_NHAN_VIEN" : "SUA_NHAN_VIEN", "NhanVien",
            $"Tài khoản {form.TenDangNhap}; chức vụ #{form.MaChucVu}; mở={form.Active}; đang làm={form.DangLam}; quyền={string.Join(",", ids)}; cấp mật khẩu tạm={!string.IsNullOrEmpty(form.TemporaryPassword)}"))
            return RedirectToAction(nameof(Employees));
        await LoadStaff(form);
        return View(form);
    }

    public async Task<IActionResult> Customers(string? q, int page = 1)
    {
        q = q?.Trim();
        var query = db.KhachHangs.AsNoTracking().Include(k => k.TaiKhoanKhachHang).AsQueryable();
        if (!string.IsNullOrEmpty(q)) query = query.Where(k => k.HoTen.Contains(q) || k.Email.Contains(q)
            || (k.SoDienThoai != null && k.SoDienThoai.Contains(q)));
        return View(await Paged(query.OrderByDescending(k => k.MaKh), q, page));
    }

    [HttpGet]
    public async Task<IActionResult> CustomerEdit(int id)
    {
        var k = await db.KhachHangs.AsNoTracking().Include(k => k.TaiKhoanKhachHang).SingleOrDefaultAsync(k => k.MaKh == id);
        if (k is null) return NotFound();
        return View(new CustomerForm
        {
            Id = id, HoTen = k.HoTen, Email = k.Email, SoDienThoai = k.SoDienThoai, DiaChi = k.DiaChi,
            Active = k.TaiKhoanKhachHang?.TrangThai ?? false, TenDangNhap = k.TaiKhoanKhachHang?.TenDangNhap
        });
    }

    [HttpPost]
    public async Task<IActionResult> CustomerEdit(CustomerForm form)
    {
        var k = await db.KhachHangs.Include(k => k.TaiKhoanKhachHang).SingleOrDefaultAsync(k => k.MaKh == form.Id);
        if (k is null) return NotFound();
        form.Email = form.Email?.Trim() ?? "";
        form.TenDangNhap = k.TaiKhoanKhachHang?.TenDangNhap;
        if (await db.KhachHangs.AnyAsync(k => k.Email == form.Email && k.MaKh != form.Id)) ModelState.AddModelError(nameof(form.Email), "Email đã được dùng.");
        if (!ModelState.IsValid) return View(form);
        k.HoTen = form.HoTen.Trim(); k.Email = form.Email; k.SoDienThoai = form.SoDienThoai; k.DiaChi = form.DiaChi;
        if (k.TaiKhoanKhachHang is not null) k.TaiKhoanKhachHang.TrangThai = form.Active;
        if (await Commit(async () => { await db.SaveChangesAsync(); return k.MaKh; }, "SUA_KHACH_HANG", "KhachHang",
            $"Khách hàng #{k.MaKh}; trạng thái tài khoản mở={form.Active}.")) return RedirectToAction(nameof(Customers));
        return View(form);
    }

    public async Task<IActionResult> Activity(string? q, int page = 1)
    {
        q = q?.Trim();
        var query = db.NhatKyHeThongs.AsNoTracking().Include(n => n.MaNvNavigation).AsQueryable();
        if (!string.IsNullOrEmpty(q)) query = query.Where(n => n.HanhDong.Contains(q)
            || (n.ChiTiet != null && n.ChiTiet.Contains(q))
            || (n.MaNvNavigation != null && n.MaNvNavigation.HoTen.Contains(q)));
        return View(await Paged(query.OrderByDescending(n => n.MaNk), q, page));
    }
}
