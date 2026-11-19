using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MotoShopWeb.Data;

namespace MotoShopWeb.Security;

public static class VehiclePermissions
{
    public const string View = "XE_XEM";
    public const string Create = "XE_THEM";
    public const string Edit = "XE_SUA";
    public const string Visibility = "XE_AN_HIEN";
    public static readonly string[] All = [View, Create, Edit, Visibility];
}

public class VehicleAccess(ApplicationDbContext db)
{
    public async Task<HashSet<string>> GetAsync(ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated != true
            || !int.TryParse(user.FindFirstValue("AccountId"), out int accountId)
            || !int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out int employeeId)) return [];
        var account = await db.TaiKhoanNhanViens.AsNoTracking()
            .Where(a => a.MaTknv == accountId && a.MaNv == employeeId && a.TrangThai
                && a.MaNvNavigation.TrangThai == "DangLam" && !a.BatBuocDoiMatKhau)
            .Select(a => new { Admin = a.MaNvNavigation.MaChucVuNavigation.ToanQuyen, a.MatKhauHash })
            .SingleOrDefaultAsync();
        if (account is null || user.FindFirstValue("PasswordStamp") != StaffIdentity.Stamp(account.MatKhauHash)) return [];
        if (account.Admin) return new HashSet<string>(VehiclePermissions.All, StringComparer.Ordinal);
        var codes = await db.NhanVienQuyens.AsNoTracking().Where(p => p.MaNv == employeeId)
            .Select(p => p.MaQuyenNavigation.KyHieu).ToListAsync();
        return new HashSet<string>(codes.Where(c => VehiclePermissions.All.Contains(c)), StringComparer.Ordinal);
    }

    public async Task<bool> CanAsync(ClaimsPrincipal user, string code)
    {
        var codes = await GetAsync(user);
        // Quyền thao tác xe luôn phải đi kèm quyền xem; không suy ra từ tên chức vụ.
        return codes.Contains(VehiclePermissions.View) && codes.Contains(code);
    }
}

public sealed record VehiclePermissionRequirement(string Code) : IAuthorizationRequirement;

public class VehiclePermissionHandler(VehicleAccess access) : AuthorizationHandler<VehiclePermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        VehiclePermissionRequirement requirement)
    {
        if (await access.CanAsync(context.User, requirement.Code)) context.Succeed(requirement);
    }
}
