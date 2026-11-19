using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MotoShopWeb.Data;
using MotoShopWeb.Models;

namespace MotoShopWeb.Security;

public static class StaffIdentity
{
    public static string Stamp(string hash) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(hash)));
    public static ClaimsPrincipal Principal(TaiKhoanNhanVien account)
    {
        var employee = account.MaNvNavigation;
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, employee.MaNv.ToString()),
            new Claim(ClaimTypes.Name, employee.HoTen),
            new Claim(ClaimTypes.Role, employee.MaChucVuNavigation.ToanQuyen ? "Admin" : "Staff"),
            new Claim("AccountId", account.MaTknv.ToString()),
            new Claim("PasswordStamp", Stamp(account.MatKhauHash)),
            new Claim("PasswordChanged", account.BatBuocDoiMatKhau ? "false" : "true")
        };
        return new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
    }

    public static bool Verify(TaiKhoanNhanVien account, string password)
    {
        try
        {
            return new PasswordHasher<TaiKhoanNhanVien>().VerifyHashedPassword(account,
                account.MatKhauHash, password) != PasswordVerificationResult.Failed;
        }
        catch (FormatException) { return false; }
        catch (ArgumentException) { return false; }
    }
    public static bool StrongPassword(string? password) => password is { Length: >= 12 and <= 128 }
        && password.Any(char.IsUpper) && password.Any(char.IsLower) && password.Any(char.IsDigit);
}

public sealed class StaffCookieEvents(ApplicationDbContext db) : CookieAuthenticationEvents
{
    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        if (!int.TryParse(context.Principal?.FindFirstValue("AccountId"), out var id))
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync();
            return;
        }
        var account = await db.TaiKhoanNhanViens.AsNoTracking()
            .Include(a => a.MaNvNavigation).ThenInclude(n => n.MaChucVuNavigation)
            .SingleOrDefaultAsync(a => a.MaTknv == id);
        if (account is null || !account.TrangThai || account.MaNvNavigation.TrangThai != "DangLam"
            || context.Principal?.FindFirstValue("PasswordStamp") != StaffIdentity.Stamp(account.MatKhauHash))
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync();
            return;
        }
        // Lấy lại chức vụ từ SQL mỗi request: khóa tài khoản / hạ quyền có hiệu lực ngay.
        context.ReplacePrincipal(StaffIdentity.Principal(account));
    }
}
