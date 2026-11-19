using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using MotoShopWeb.Data;
using MotoShopWeb.Models;
using MotoShopWeb.Security;
using MotoShopWeb.ViewModels;

namespace MotoShopWeb.Controllers;

[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class StaffController(ApplicationDbContext db) : Controller
{
    [AllowAnonymous, HttpGet]
    public IActionResult Login() => View(new LoginForm());

    [AllowAnonymous, HttpPost, EnableRateLimiting("staff-login")]
    public async Task<IActionResult> Login(LoginForm form)
    {
        if (!ModelState.IsValid) return View(form);
        var account = await db.TaiKhoanNhanViens
            .Include(a => a.MaNvNavigation).ThenInclude(n => n.MaChucVuNavigation)
            .SingleOrDefaultAsync(a => a.TenDangNhap == form.Username.Trim());
        if (account is null || !StaffIdentity.Verify(account, form.Password)
            || !account.TrangThai || account.MaNvNavigation.TrangThai != "DangLam")
        {
            db.NhatKyHeThongs.Add(new NhatKyHeThong
            {
                MaNv = account?.MaNv, HanhDong = "DANG_NHAP_THAT_BAI", BangTacDong = "TaiKhoanNhanVien",
                MaBanGhi = account?.MaTknv, ChiTiet = "Đăng nhập không thành công: " + form.Username.Trim(), ThoiGian = DateTime.Now
            });
            await db.SaveChangesAsync();
            ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng, hoặc tài khoản không được phép đăng nhập.");
            form.Password = "";
            ModelState.Remove(nameof(form.Password));
            return View(form);
        }
        db.NhatKyHeThongs.Add(new NhatKyHeThong
        {
            MaNv = account.MaNv, HanhDong = "DANG_NHAP", BangTacDong = "TaiKhoanNhanVien",
            MaBanGhi = account.MaTknv, ChiTiet = "Đăng nhập tài khoản nhân viên thành công.", ThoiGian = DateTime.Now
        });
        await db.SaveChangesAsync();
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            StaffIdentity.Principal(account), new AuthenticationProperties { IsPersistent = false });
        if (account.BatBuocDoiMatKhau) return RedirectToAction(nameof(ChangePassword));
        return account.MaNvNavigation.MaChucVuNavigation.ToanQuyen
            ? RedirectToAction("Index", "Admin") : RedirectToAction("Index", "Workspace");
    }

    [Authorize, HttpPost]
    public async Task<IActionResult> Logout()
    {
        db.NhatKyHeThongs.Add(new NhatKyHeThong
        {
            MaNv = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!), HanhDong = "DANG_XUAT",
            BangTacDong = "TaiKhoanNhanVien", ChiTiet = "Đăng xuất.", ThoiGian = DateTime.Now
        });
        await db.SaveChangesAsync();
        await HttpContext.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    [Authorize, HttpGet]
    public IActionResult ChangePassword() => View(new PasswordForm());

    [Authorize, HttpPost, EnableRateLimiting("staff-login")]
    public async Task<IActionResult> ChangePassword(PasswordForm form)
    {
        if (!StaffIdentity.StrongPassword(form.NewPassword))
            ModelState.AddModelError(nameof(form.NewPassword), "Mật khẩu cần 12–128 ký tự, có chữ hoa, chữ thường và số.");
        if (form.CurrentPassword == form.NewPassword)
            ModelState.AddModelError(nameof(form.NewPassword), "Mật khẩu mới phải khác mật khẩu hiện tại.");
        if (!ModelState.IsValid) return View(form);
        int id = int.Parse(User.FindFirstValue("AccountId")!);
        var account = await db.TaiKhoanNhanViens.Include(a => a.MaNvNavigation)
            .ThenInclude(n => n.MaChucVuNavigation).SingleAsync(a => a.MaTknv == id);
        if (!StaffIdentity.Verify(account, form.CurrentPassword))
        {
            ModelState.AddModelError(nameof(form.CurrentPassword), "Mật khẩu hiện tại chưa đúng.");
            return View(form);
        }
        await using var tx = await db.Database.BeginTransactionAsync();
        account.MatKhauHash = new PasswordHasher<TaiKhoanNhanVien>().HashPassword(account, form.NewPassword);
        account.BatBuocDoiMatKhau = false;
        account.NgayDoiMatKhau = DateTime.Now;
        db.NhatKyHeThongs.Add(new NhatKyHeThong
        {
            MaNv = account.MaNv, HanhDong = "DOI_MAT_KHAU", BangTacDong = "TaiKhoanNhanVien",
            MaBanGhi = id, ChiTiet = "Người dùng tự đổi mật khẩu.", ThoiGian = DateTime.Now
        });
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, StaffIdentity.Principal(account));
        TempData["Success"] = "Đổi mật khẩu thành công.";
        return account.MaNvNavigation.MaChucVuNavigation.ToanQuyen
            ? RedirectToAction("Index", "Admin") : RedirectToAction("Index", "Workspace");
    }

    [AllowAnonymous]
    public IActionResult Denied()
    {
        if (User.Identity?.IsAuthenticated == true && User.FindFirstValue("PasswordChanged") == "false")
            return RedirectToAction(nameof(ChangePassword));
        Response.StatusCode = 403;
        return View();
    }

    [AllowAnonymous]
    public IActionResult Error()
    {
        Response.StatusCode = 500;
        return View();
    }
}
