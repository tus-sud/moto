using System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MotoShopWeb.Data;
using MotoShopWeb.Models;

namespace MotoShopWeb.Security;

public static class AdminBootstrap
{
    public static async Task InitializeAsync(ApplicationDbContext db, string? password)
    {
        if (string.IsNullOrWhiteSpace(password)) return;
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var account = await db.TaiKhoanNhanViens
            .Include(a => a.MaNvNavigation).ThenInclude(n => n.MaChucVuNavigation)
            .SingleOrDefaultAsync(a => a.TenDangNhap == "admin");

        // Không đặt lại mật khẩu hay nâng quyền một tài khoản đã sử dụng.
        if (account is not null && account.MatKhauHash != "THAY_BANG_HASH_THAT") return;
        if (!StaffIdentity.StrongPassword(password))
            throw new InvalidOperationException("AdminBootstrap:Password cần 12–128 ký tự, có chữ hoa, chữ thường và số.");

        if (account is null)
        {
            if (await db.TaiKhoanNhanViens.AnyAsync(a => a.TrangThai
                && a.MaNvNavigation.TrangThai == "DangLam" && a.MaNvNavigation.MaChucVuNavigation.ToanQuyen))
                throw new InvalidOperationException("Đã có tài khoản quản lý. Hãy dùng tài khoản đó, không khởi tạo admin mới.");
            var role = await db.ChucVus.FirstOrDefaultAsync(c => c.ToanQuyen);
            if (role is null)
            {
                role = new ChucVu { TenChucVu = "Quản lý", LuongCoBan = 0, ToanQuyen = true };
                db.ChucVus.Add(role);
                await db.SaveChangesAsync();
            }
            var employee = new NhanVien
            {
                MaChucVu = role.MaChucVu,
                HoTen = "Quản trị hệ thống",
                Email = "admin@motoshop.local",
                HeSoLuong = 1,
                TrangThai = "DangLam",
                NgayVaoLam = DateOnly.FromDateTime(DateTime.Today)
            };
            db.NhanViens.Add(employee);
            await db.SaveChangesAsync();
            account = new TaiKhoanNhanVien
            {
                MaNv = employee.MaNv,
                TenDangNhap = "admin",
                TrangThai = true,
                MatKhauHash = "THAY_BANG_HASH_THAT"
            };
            db.TaiKhoanNhanViens.Add(account);
        }
        else if (!account.TrangThai || account.MaNvNavigation.TrangThai != "DangLam"
            || !account.MaNvNavigation.MaChucVuNavigation.ToanQuyen)
            throw new InvalidOperationException("Tài khoản admin mẫu không còn là quản lý hoạt động; không tự nâng quyền.");

        account.MatKhauHash = new PasswordHasher<TaiKhoanNhanVien>().HashPassword(account, password);
        account.BatBuocDoiMatKhau = false;
        account.NgayDoiMatKhau = DateTime.Now;
        await db.SaveChangesAsync();
        foreach (var title in new[] { "Nhân viên bán hàng", "Nhân viên vận hành", "Kỹ thuật viên" })
            if (!await db.ChucVus.AnyAsync(c => c.TenChucVu == title))
                db.ChucVus.Add(new ChucVu { TenChucVu = title, LuongCoBan = 0, ToanQuyen = false });
        db.NhatKyHeThongs.Add(new NhatKyHeThong
        {
            MaNv = account.MaNv,
            HanhDong = "KHOI_TAO_ADMIN",
            BangTacDong = "TaiKhoanNhanVien",
            MaBanGhi = account.MaTknv,
            ChiTiet = "Khởi tạo mật khẩu admin lần đầu từ cấu hình máy phát triển.",
            ThoiGian = DateTime.Now
        });
        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }
}
