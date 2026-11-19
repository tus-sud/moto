using Microsoft.EntityFrameworkCore;

namespace MotoShopWeb.Data;

public static class SchemaInitializer
{
    private static bool _initialized;

    public static async Task EnsureCompatibleSchemaAsync(ApplicationDbContext db)
    {
        if (_initialized) return;
        await db.Database.EnsureCreatedAsync();

        const string sql = """
            -- 0. Bảng HangXe, LoaiXe, Xe, MauXe, AnhXe
            IF OBJECT_ID(N'dbo.HangXe', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.HangXe (
                    MaHang INT IDENTITY(1,1) PRIMARY KEY,
                    TenHang NVARCHAR(100) NOT NULL UNIQUE,
                    QuocGia NVARCHAR(100) NULL,
                    Logo NVARCHAR(255) NULL,
                    MoTa NVARCHAR(500) NULL
                );
            END;

            IF OBJECT_ID(N'dbo.LoaiXe', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.LoaiXe (
                    MaLoai INT IDENTITY(1,1) PRIMARY KEY,
                    TenLoai NVARCHAR(100) NOT NULL UNIQUE,
                    MoTa NVARCHAR(500) NULL
                );
            END;

            IF OBJECT_ID(N'dbo.Xe', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.Xe (
                    MaXe INT IDENTITY(1,1) PRIMARY KEY,
                    MaHang INT NOT NULL REFERENCES dbo.HangXe(MaHang),
                    MaLoai INT NOT NULL REFERENCES dbo.LoaiXe(MaLoai),
                    TenXe NVARCHAR(200) NOT NULL,
                    NamSanXuat INT NULL,
                    GiaNiemYet DECIMAL(18,0) NOT NULL DEFAULT(0),
                    DungTichCC INT NULL,
                    CongSuatKW DECIMAL(6,1) NULL,
                    HopSo NVARCHAR(50) NULL,
                    KieuDanDong NVARCHAR(50) NULL,
                    SoCho INT NOT NULL DEFAULT(2),
                    MoTa NVARCHAR(500) NULL,
                    HienThi BIT NOT NULL DEFAULT(1)
                );
            END;

            IF OBJECT_ID(N'dbo.MauXe', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.MauXe (
                    MaMau INT IDENTITY(1,1) PRIMARY KEY,
                    MaXe INT NOT NULL REFERENCES dbo.Xe(MaXe),
                    TenMau NVARCHAR(50) NOT NULL,
                    MaHex VARCHAR(7) NULL
                );
            END;

            IF OBJECT_ID(N'dbo.AnhXe', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.AnhXe (
                    MaAnh INT IDENTITY(1,1) PRIMARY KEY,
                    MaXe INT NOT NULL REFERENCES dbo.Xe(MaXe),
                    MaMau INT NULL,
                    DuongDan NVARCHAR(255) NOT NULL,
                    LaAnhChinh BIT NOT NULL DEFAULT(0),
                    ThuTu INT NOT NULL DEFAULT(0)
                );
            END;

            -- 1. Bảng ChucVu
            IF OBJECT_ID(N'dbo.ChucVu', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.ChucVu (
                    MaChucVu INT IDENTITY(1,1) PRIMARY KEY,
                    TenChucVu NVARCHAR(100) NOT NULL UNIQUE,
                    LuongCoBan DECIMAL(18,0) NOT NULL DEFAULT(0),
                    ToanQuyen BIT NOT NULL DEFAULT(0),
                    MoTa NVARCHAR(500) NULL
                );
            END
            ELSE
            BEGIN
                IF COL_LENGTH(N'dbo.ChucVu', N'LuongCoBan') IS NULL
                    ALTER TABLE dbo.ChucVu ADD LuongCoBan DECIMAL(18,0) NOT NULL DEFAULT(0);
                IF COL_LENGTH(N'dbo.ChucVu', N'ToanQuyen') IS NULL
                    ALTER TABLE dbo.ChucVu ADD ToanQuyen BIT NOT NULL DEFAULT(0);
                IF COL_LENGTH(N'dbo.ChucVu', N'MoTa') IS NULL
                    ALTER TABLE dbo.ChucVu ADD MoTa NVARCHAR(500) NULL;
            END;

            -- 2. Bảng KhachHang
            IF OBJECT_ID(N'dbo.KhachHang', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.KhachHang (
                    MaKH INT IDENTITY(1,1) PRIMARY KEY,
                    HoTen NVARCHAR(100) NOT NULL,
                    Email VARCHAR(150) NOT NULL UNIQUE,
                    SoDienThoai VARCHAR(15) NULL,
                    DiaChi NVARCHAR(255) NULL,
                    NgaySinh DATE NULL,
                    NgayDangKy DATETIME NOT NULL DEFAULT(GETDATE())
                );
            END
            ELSE
            BEGIN
                IF COL_LENGTH(N'dbo.KhachHang', N'SoDienThoai') IS NULL
                    ALTER TABLE dbo.KhachHang ADD SoDienThoai VARCHAR(15) NULL;
                IF COL_LENGTH(N'dbo.KhachHang', N'DiaChi') IS NULL
                    ALTER TABLE dbo.KhachHang ADD DiaChi NVARCHAR(255) NULL;
                IF COL_LENGTH(N'dbo.KhachHang', N'NgaySinh') IS NULL
                    ALTER TABLE dbo.KhachHang ADD NgaySinh DATE NULL;
                IF COL_LENGTH(N'dbo.KhachHang', N'NgayDangKy') IS NULL
                    ALTER TABLE dbo.KhachHang ADD NgayDangKy DATETIME NOT NULL DEFAULT(GETDATE());
            END;

            -- Nới lỏng các cột cũ không còn dùng trong bảng KhachHang (nếu DB cũ có cột MatKhau / TenDangNhap NOT NULL)
            DECLARE @relaxKhSql NVARCHAR(MAX) = N'';
            SELECT @relaxKhSql += N'ALTER TABLE dbo.KhachHang ALTER COLUMN ' + QUOTENAME(c.name) + N' '
                + TYPE_NAME(c.user_type_id)
                + CASE WHEN TYPE_NAME(c.user_type_id) IN (N'varchar', N'nvarchar', N'char', N'nchar')
                       THEN N'(' + CASE WHEN c.max_length = -1 THEN N'MAX'
                                        WHEN TYPE_NAME(c.user_type_id) IN (N'nvarchar', N'nchar') THEN CAST(c.max_length / 2 AS NVARCHAR(10))
                                        ELSE CAST(c.max_length AS NVARCHAR(10)) END + N')'
                       ELSE N'' END + N' NULL;'
            FROM sys.columns c
            WHERE c.object_id = OBJECT_ID(N'dbo.KhachHang')
              AND c.is_identity = 0
              AND c.is_computed = 0
              AND c.is_nullable = 0
              AND c.name NOT IN (N'MaKH', N'HoTen', N'Email', N'NgayDangKy');
            IF LEN(@relaxKhSql) > 0 EXEC sp_executesql @relaxKhSql;

            -- 3. Bảng TaiKhoanKhachHang
            IF OBJECT_ID(N'dbo.TaiKhoanKhachHang', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.TaiKhoanKhachHang (
                    MaTKKH INT IDENTITY(1,1) PRIMARY KEY,
                    MaKH INT NOT NULL UNIQUE REFERENCES dbo.KhachHang(MaKH),
                    TenDangNhap NVARCHAR(50) NOT NULL UNIQUE,
                    MatKhauHash VARCHAR(255) NOT NULL,
                    NgayTao DATETIME NOT NULL DEFAULT(GETDATE()),
                    TrangThai BIT NOT NULL DEFAULT(1)
                );
            END
            ELSE
            BEGIN
                IF COL_LENGTH(N'dbo.TaiKhoanKhachHang', N'NgayTao') IS NULL
                    ALTER TABLE dbo.TaiKhoanKhachHang ADD NgayTao DATETIME NOT NULL DEFAULT(GETDATE());
                IF COL_LENGTH(N'dbo.TaiKhoanKhachHang', N'TrangThai') IS NULL
                    ALTER TABLE dbo.TaiKhoanKhachHang ADD TrangThai BIT NOT NULL DEFAULT(1);
            END;

            -- 4. Bảng NhanVien
            IF OBJECT_ID(N'dbo.NhanVien', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.NhanVien (
                    MaNV INT IDENTITY(1,1) PRIMARY KEY,
                    MaChucVu INT NOT NULL REFERENCES dbo.ChucVu(MaChucVu),
                    MaQuanLy INT NULL REFERENCES dbo.NhanVien(MaNV),
                    HoTen NVARCHAR(100) NOT NULL,
                    NgaySinh DATE NULL,
                    SoDienThoai VARCHAR(15) NULL,
                    Email VARCHAR(150) NOT NULL UNIQUE,
                    DiaChi NVARCHAR(255) NULL,
                    NgayVaoLam DATE NOT NULL DEFAULT(CONVERT(DATE, GETDATE())),
                    HeSoLuong DECIMAL(4,2) NOT NULL DEFAULT(1),
                    TrangThai NVARCHAR(20) NOT NULL DEFAULT(N'DangLam')
                );
            END
            ELSE
            BEGIN
                IF COL_LENGTH(N'dbo.NhanVien', N'MaQuanLy') IS NULL
                    ALTER TABLE dbo.NhanVien ADD MaQuanLy INT NULL;
                IF COL_LENGTH(N'dbo.NhanVien', N'NgaySinh') IS NULL
                    ALTER TABLE dbo.NhanVien ADD NgaySinh DATE NULL;
                IF COL_LENGTH(N'dbo.NhanVien', N'SoDienThoai') IS NULL
                    ALTER TABLE dbo.NhanVien ADD SoDienThoai VARCHAR(15) NULL;
                IF COL_LENGTH(N'dbo.NhanVien', N'DiaChi') IS NULL
                    ALTER TABLE dbo.NhanVien ADD DiaChi NVARCHAR(255) NULL;
                IF COL_LENGTH(N'dbo.NhanVien', N'NgayVaoLam') IS NULL
                    ALTER TABLE dbo.NhanVien ADD NgayVaoLam DATE NOT NULL DEFAULT(CONVERT(DATE, GETDATE()));
                IF COL_LENGTH(N'dbo.NhanVien', N'HeSoLuong') IS NULL
                    ALTER TABLE dbo.NhanVien ADD HeSoLuong DECIMAL(4,2) NOT NULL DEFAULT(1);
                IF COL_LENGTH(N'dbo.NhanVien', N'TrangThai') IS NULL
                    ALTER TABLE dbo.NhanVien ADD TrangThai NVARCHAR(20) NOT NULL DEFAULT(N'DangLam');
            END;

            -- 5. Bảng TaiKhoanNhanVien
            IF OBJECT_ID(N'dbo.TaiKhoanNhanVien', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.TaiKhoanNhanVien (
                    MaTKNV INT IDENTITY(1,1) PRIMARY KEY,
                    MaNV INT NOT NULL UNIQUE REFERENCES dbo.NhanVien(MaNV),
                    MaNguoiCap INT NULL REFERENCES dbo.NhanVien(MaNV),
                    TenDangNhap NVARCHAR(50) NOT NULL UNIQUE,
                    MatKhauHash VARCHAR(255) NOT NULL,
                    NgayCap DATETIME NOT NULL DEFAULT(GETDATE()),
                    BatBuocDoiMatKhau BIT NOT NULL DEFAULT(1),
                    NgayDoiMatKhau DATETIME NULL,
                    TrangThai BIT NOT NULL DEFAULT(1)
                );
            END;

            -- 6. Bảng Quyen & NhanVienQuyen & NhatKyHeThong
            IF OBJECT_ID(N'dbo.Quyen', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.Quyen (
                    MaQuyen INT IDENTITY(1,1) PRIMARY KEY,
                    KyHieu VARCHAR(50) NOT NULL UNIQUE,
                    TenQuyen NVARCHAR(100) NOT NULL,
                    NhomChucNang NVARCHAR(50) NULL,
                    MoTa NVARCHAR(255) NULL
                );
            END;

            IF OBJECT_ID(N'dbo.NhanVienQuyen', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.NhanVienQuyen (
                    MaNV INT NOT NULL REFERENCES dbo.NhanVien(MaNV),
                    MaQuyen INT NOT NULL REFERENCES dbo.Quyen(MaQuyen),
                    MaNguoiCap INT NOT NULL REFERENCES dbo.NhanVien(MaNV),
                    NgayCap DATETIME NOT NULL DEFAULT(GETDATE()),
                    CONSTRAINT PK_NhanVienQuyen PRIMARY KEY (MaNV, MaQuyen)
                );
            END;

            IF OBJECT_ID(N'dbo.NhatKyHeThong', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.NhatKyHeThong (
                    MaNK INT IDENTITY(1,1) PRIMARY KEY,
                    MaNV INT NULL REFERENCES dbo.NhanVien(MaNV),
                    HanhDong NVARCHAR(50) NOT NULL,
                    BangTacDong NVARCHAR(50) NULL,
                    MaBanGhi INT NULL,
                    ChiTiet NVARCHAR(1000) NULL,
                    ThoiGian DATETIME NOT NULL DEFAULT(GETDATE())
                );
            END;

            -- 7. Bảng KhuyenMai, DonHang, ChiTietDonHang, LichSuDonHang, ThanhToan
            IF OBJECT_ID(N'dbo.KhuyenMai', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.KhuyenMai (
                    MaKM INT IDENTITY(1,1) PRIMARY KEY,
                    MaCode VARCHAR(30) NOT NULL UNIQUE,
                    LoaiGiam NVARCHAR(20) NOT NULL,
                    GiaTri DECIMAL(18,2) NOT NULL,
                    NgayBatDau DATE NOT NULL,
                    NgayKetThuc DATE NOT NULL,
                    TrangThai BIT NOT NULL DEFAULT(1)
                );
            END;

            IF OBJECT_ID(N'dbo.DonHang', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.DonHang (
                    MaDonHang INT IDENTITY(1,1) PRIMARY KEY,
                    MaKH INT NOT NULL REFERENCES dbo.KhachHang(MaKH),
                    MaKM INT NULL REFERENCES dbo.KhuyenMai(MaKM),
                    MaNV INT NULL REFERENCES dbo.NhanVien(MaNV),
                    NgayDat DATETIME NOT NULL DEFAULT(GETDATE()),
                    TrangThai NVARCHAR(30) NOT NULL DEFAULT(N'ChoXacNhan'),
                    TenNguoiNhan NVARCHAR(100) NOT NULL DEFAULT(N''),
                    SDTNguoiNhan VARCHAR(15) NOT NULL DEFAULT(''),
                    DiaChiGiao NVARCHAR(255) NOT NULL DEFAULT(N''),
                    PhiVanChuyen DECIMAL(18,0) NOT NULL DEFAULT(0),
                    GiamGia DECIMAL(18,0) NOT NULL DEFAULT(0),
                    TongTien DECIMAL(18,0) NOT NULL DEFAULT(0),
                    GhiChu NVARCHAR(500) NULL
                );
            END
            ELSE
            BEGIN
                IF COL_LENGTH(N'dbo.DonHang', N'TenNguoiNhan') IS NULL
                    ALTER TABLE dbo.DonHang ADD TenNguoiNhan NVARCHAR(100) NOT NULL DEFAULT(N'');
                IF COL_LENGTH(N'dbo.DonHang', N'SDTNguoiNhan') IS NULL
                    ALTER TABLE dbo.DonHang ADD SDTNguoiNhan VARCHAR(15) NOT NULL DEFAULT('');
                IF COL_LENGTH(N'dbo.DonHang', N'DiaChiGiao') IS NULL
                    ALTER TABLE dbo.DonHang ADD DiaChiGiao NVARCHAR(255) NOT NULL DEFAULT(N'');
                IF COL_LENGTH(N'dbo.DonHang', N'PhiVanChuyen') IS NULL
                    ALTER TABLE dbo.DonHang ADD PhiVanChuyen DECIMAL(18,0) NOT NULL DEFAULT(0);
                IF COL_LENGTH(N'dbo.DonHang', N'GiamGia') IS NULL
                    ALTER TABLE dbo.DonHang ADD GiamGia DECIMAL(18,0) NOT NULL DEFAULT(0);
                IF COL_LENGTH(N'dbo.DonHang', N'TongTien') IS NULL
                    ALTER TABLE dbo.DonHang ADD TongTien DECIMAL(18,0) NOT NULL DEFAULT(0);
                IF COL_LENGTH(N'dbo.DonHang', N'GhiChu') IS NULL
                    ALTER TABLE dbo.DonHang ADD GhiChu NVARCHAR(500) NULL;
            END;

            IF OBJECT_ID(N'dbo.ChiTietDonHang', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.ChiTietDonHang (
                    MaCT INT IDENTITY(1,1) PRIMARY KEY,
                    MaDonHang INT NOT NULL REFERENCES dbo.DonHang(MaDonHang),
                    MaXe INT NOT NULL,
                    MaMau INT NOT NULL,
                    SoLuong INT NOT NULL DEFAULT(1),
                    DonGia DECIMAL(18,0) NOT NULL DEFAULT(0),
                    ThanhTien AS ([SoLuong]*[DonGia]) PERSISTED
                );
            END;

            IF OBJECT_ID(N'dbo.LichSuDonHang', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.LichSuDonHang (
                    MaLS INT IDENTITY(1,1) PRIMARY KEY,
                    MaDonHang INT NOT NULL REFERENCES dbo.DonHang(MaDonHang),
                    MaNV INT NULL REFERENCES dbo.NhanVien(MaNV),
                    TrangThai NVARCHAR(30) NOT NULL,
                    ThoiGian DATETIME NOT NULL DEFAULT(GETDATE()),
                    GhiChu NVARCHAR(500) NULL
                );
            END;

            IF OBJECT_ID(N'dbo.ThanhToan', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.ThanhToan (
                    MaThanhToan INT IDENTITY(1,1) PRIMARY KEY,
                    MaDonHang INT NOT NULL REFERENCES dbo.DonHang(MaDonHang),
                    LoaiThanhToan NVARCHAR(30) NOT NULL,
                    PhuongThuc NVARCHAR(50) NOT NULL,
                    SoTien DECIMAL(18,0) NOT NULL,
                    NgayThanhToan DATETIME NOT NULL DEFAULT(GETDATE()),
                    MaGiaoDich VARCHAR(100) NULL,
                    TrangThai NVARCHAR(30) NOT NULL DEFAULT(N'ThanhCong')
                );
            END;

            -- 8. Bảng YeuThich, LichLaiThu, DanhGia
            IF OBJECT_ID(N'dbo.YeuThich', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.YeuThich (
                    MaKH INT NOT NULL REFERENCES dbo.KhachHang(MaKH),
                    MaXe INT NOT NULL REFERENCES dbo.Xe(MaXe),
                    NgayThem DATETIME NOT NULL DEFAULT(GETDATE()),
                    CONSTRAINT PK_YeuThich PRIMARY KEY (MaKH, MaXe)
                );
            END;

            IF OBJECT_ID(N'dbo.LichLaiThu', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.LichLaiThu (
                    MaLich INT IDENTITY(1,1) PRIMARY KEY,
                    MaKH INT NOT NULL REFERENCES dbo.KhachHang(MaKH),
                    MaXe INT NOT NULL REFERENCES dbo.Xe(MaXe),
                    NgayHen DATETIME NOT NULL,
                    TrangThai NVARCHAR(30) NOT NULL DEFAULT(N'ChoXacNhan'),
                    GhiChu NVARCHAR(500) NULL
                );
            END;

            IF OBJECT_ID(N'dbo.DanhGia', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.DanhGia (
                    MaDanhGia INT IDENTITY(1,1) PRIMARY KEY,
                    MaKH INT NOT NULL REFERENCES dbo.KhachHang(MaKH),
                    MaXe INT NOT NULL REFERENCES dbo.Xe(MaXe),
                    MaCT INT NULL,
                    SoSao INT NOT NULL,
                    NoiDung NVARCHAR(1000) NULL,
                    NgayDanhGia DATETIME NOT NULL DEFAULT(GETDATE())
                );
            END;

            -- 9. Bảng GoiBaoHanh & dữ liệu gói bảo hành mẫu
            IF OBJECT_ID(N'dbo.GoiBaoHanh', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.GoiBaoHanh (
                    MaGoiBH INT IDENTITY(1,1) PRIMARY KEY,
                    TenGoi NVARCHAR(100) NOT NULL,
                    SoThang INT NOT NULL DEFAULT(36),
                    SoKm INT NULL,
                    GiaTien DECIMAL(18,0) NOT NULL DEFAULT(0),
                    MoTa NVARCHAR(500) NULL
                );
            END;

            IF NOT EXISTS (SELECT 1 FROM dbo.GoiBaoHanh)
            BEGIN
                INSERT INTO dbo.GoiBaoHanh (TenGoi, SoThang, SoKm, GiaTien, MoTa) VALUES
                (N'Bảo hành Tiêu chuẩn Chính hãng', 36, 30000, 0, N'Bảo hành động cơ, hộp số, khung sườn và hệ thống phun xăng điện tử theo tiêu chuẩn hãng 36 tháng hoặc 30.000 km. Miễn phí 3 lần kiểm tra định kỳ đầu tiên.'),
                (N'Bảo hành Mở rộng VIP Care+', 48, 50000, 3500000, N'Gia hạn bảo hành toàn diện lên 48 tháng / 50.000 km cho động cơ, ECU, hệ thống điện, phuộc và phanh ABS. Tặng 2 lần thay dầu tổng hợp và cứu hộ 24/7 bán kính 100 km.'),
                (N'Bảo vệ Toàn diện Rider Shield 24/7', 24, 99999, 5900000, N'Bảo vệ rủi ro va chạm, ngập nước, hư hỏng phụ tùng chính hãng. Hỗ trợ đến 80% chi phí sửa chữa, cứu hộ toàn quốc 24/7 không giới hạn và tặng phủ Ceramic khi giao xe.');
            END;

            -- 10. Tự động bổ sung màu xe cho các mẫu xe chưa có màu (như Kawasaki Z650 2026, Kawasaki Z900 2026) để luôn hiện nút Thêm vào giỏ hàng
            INSERT INTO dbo.MauXe (MaXe, TenMau, MaHex)
            SELECT x.MaXe, N'Xanh Lime / Đen Thể thao', '#55b82e'
            FROM dbo.Xe x
            WHERE x.TenXe LIKE N'%Kawasaki%'
              AND NOT EXISTS (SELECT 1 FROM dbo.MauXe m WHERE m.MaXe = x.MaXe AND m.TenMau = N'Xanh Lime / Đen Thể thao');

            INSERT INTO dbo.MauXe (MaXe, TenMau, MaHex)
            SELECT x.MaXe, N'Đen Nhám Metallic', '#1f2421'
            FROM dbo.Xe x
            WHERE NOT EXISTS (SELECT 1 FROM dbo.MauXe m WHERE m.MaXe = x.MaXe);

            INSERT INTO dbo.MauXe (MaXe, TenMau, MaHex)
            SELECT x.MaXe, N'Xám Đỏ Sport', '#7a1c1c'
            FROM dbo.Xe x
            WHERE (SELECT COUNT(*) FROM dbo.MauXe m WHERE m.MaXe = x.MaXe) < 2;
            """;

        await db.Database.ExecuteSqlRawAsync(sql);
        _initialized = true;
    }
}
