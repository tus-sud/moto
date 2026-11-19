-- Script tự động bổ sung các bảng còn thiếu (TaiKhoanKhachHang, TaiKhoanNhanVien, Quyen, DonHang,...) cho database WebBanXeMoto
-- Bạn có thể chạy trực tiếp file này trong SSMS trên database WebBanXeMoto, HOẶC chỉ cần Restart lại ứng dụng web (code đã tự động chạy khi khởi động).

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
END;

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
END;

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

