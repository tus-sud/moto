using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MotoShopWeb.Models;

namespace MotoShopWeb.Data;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AnhXe> AnhXes { get; set; }

    public virtual DbSet<BaoHanh> BaoHanhs { get; set; }

    public virtual DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }

    public virtual DbSet<ChiTietHoaDon> ChiTietHoaDons { get; set; }

    public virtual DbSet<ChucVu> ChucVus { get; set; }

    public virtual DbSet<DanhGia> DanhGia { get; set; }

    public virtual DbSet<DonHang> DonHangs { get; set; }

    public virtual DbSet<GoiBaoHanh> GoiBaoHanhs { get; set; }

    public virtual DbSet<HangXe> HangXes { get; set; }

    public virtual DbSet<HoaDon> HoaDons { get; set; }

    public virtual DbSet<KhachHang> KhachHangs { get; set; }

    public virtual DbSet<KhuyenMai> KhuyenMais { get; set; }

    public virtual DbSet<LichLaiThu> LichLaiThus { get; set; }

    public virtual DbSet<LichSuDonHang> LichSuDonHangs { get; set; }

    public virtual DbSet<LoaiXe> LoaiXes { get; set; }

    public virtual DbSet<MauXe> MauXes { get; set; }

    public virtual DbSet<NhaCungCap> NhaCungCaps { get; set; }

    public virtual DbSet<NhanVien> NhanViens { get; set; }

    public virtual DbSet<NhanVienQuyen> NhanVienQuyens { get; set; }

    public virtual DbSet<NhatKyHeThong> NhatKyHeThongs { get; set; }

    public virtual DbSet<PhieuBaoHanh> PhieuBaoHanhs { get; set; }

    public virtual DbSet<PhieuNhap> PhieuNhaps { get; set; }

    public virtual DbSet<Quyen> Quyens { get; set; }

    public virtual DbSet<TaiKhoanKhachHang> TaiKhoanKhachHangs { get; set; }

    public virtual DbSet<TaiKhoanNhanVien> TaiKhoanNhanViens { get; set; }

    public virtual DbSet<ThanhToan> ThanhToans { get; set; }

    public virtual DbSet<Xe> Xes { get; set; }

    public virtual DbSet<XeThucTe> XeThucTes { get; set; }

    public virtual DbSet<YeuThich> YeuThiches { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AnhXe>(entity =>
        {
            entity.HasKey(e => e.MaAnh).HasName("PK__AnhXe__356240DF4CD3015E");

            entity.ToTable("AnhXe");

            entity.HasIndex(e => e.MaXe, "UX_AnhXe_AnhChinh")
                .IsUnique()
                .HasFilter("([LaAnhChinh]=(1))");

            entity.Property(e => e.DuongDan).HasMaxLength(255);

            entity.HasOne(d => d.MaXeNavigation).WithMany(p => p.AnhXes)
                .HasForeignKey(d => d.MaXe)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AnhXe__MaXe__5FB337D6");

            entity.HasOne(d => d.MauXe).WithMany(p => p.AnhXes)
                .HasPrincipalKey(p => new { p.MaMau, p.MaXe })
                .HasForeignKey(d => new { d.MaMau, d.MaXe })
                .HasConstraintName("FK__AnhXe__628FA481");
        });

        modelBuilder.Entity<BaoHanh>(entity =>
        {
            entity.HasKey(e => e.MaBh).HasName("PK__BaoHanh__272475A39E58899E");

            entity.ToTable("BaoHanh");

            entity.Property(e => e.MaBh).HasColumnName("MaBH");
            entity.Property(e => e.GhiChu).HasMaxLength(500);
            entity.Property(e => e.MaGoiBh).HasColumnName("MaGoiBH");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(30)
                .HasDefaultValue("ConHan");

            entity.HasOne(d => d.MaGoiBhNavigation).WithMany(p => p.BaoHanhs)
                .HasForeignKey(d => d.MaGoiBh)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BaoHanh__MaGoiBH__5CA1C101");

            entity.HasOne(d => d.MaXeThucTeNavigation).WithMany(p => p.BaoHanhs)
                .HasForeignKey(d => d.MaXeThucTe)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BaoHanh__MaXeThu__5BAD9CC8");
        });

        modelBuilder.Entity<ChiTietDonHang>(entity =>
        {
            entity.HasKey(e => e.MaCt).HasName("PK__ChiTietD__27258E745F39A180");

            entity.ToTable("ChiTietDonHang", tb => tb.HasTrigger("TR_ChiTietDonHang_TongTien"));

            entity.Property(e => e.MaCt).HasColumnName("MaCT");
            entity.Property(e => e.DonGia).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.ThanhTien)
                .HasComputedColumnSql("([SoLuong]*[DonGia])", true)
                .HasColumnType("decimal(29, 0)");

            entity.HasOne(d => d.MaDonHangNavigation).WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.MaDonHang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietDo__MaDon__3587F3E0");

            entity.HasOne(d => d.MauXe).WithMany(p => p.ChiTietDonHangs)
                .HasPrincipalKey(p => new { p.MaMau, p.MaXe })
                .HasForeignKey(d => new { d.MaMau, d.MaXe })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietDonHang__3864608B");
        });

        modelBuilder.Entity<ChiTietHoaDon>(entity =>
        {
            entity.HasKey(e => e.MaCthd).HasName("PK__ChiTietH__1E4FA77135EC08B5");

            entity.ToTable("ChiTietHoaDon", tb => tb.HasTrigger("TR_ChiTietHoaDon_Insert"));

            entity.HasIndex(e => e.MaXeThucTe, "UQ__ChiTietH__175A504BA724B7C0").IsUnique();

            entity.Property(e => e.MaCthd).HasColumnName("MaCTHD");
            entity.Property(e => e.DonGia).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.MaCt).HasColumnName("MaCT");
            entity.Property(e => e.MaHd).HasColumnName("MaHD");

            entity.HasOne(d => d.MaCtNavigation).WithMany(p => p.ChiTietHoaDons)
                .HasForeignKey(d => d.MaCt)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietHoa__MaCT__51300E55");

            entity.HasOne(d => d.MaHdNavigation).WithMany(p => p.ChiTietHoaDons)
                .HasForeignKey(d => d.MaHd)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietHoa__MaHD__503BEA1C");

            entity.HasOne(d => d.MaXeThucTeNavigation).WithOne(p => p.ChiTietHoaDon)
                .HasForeignKey<ChiTietHoaDon>(d => d.MaXeThucTe)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietHo__MaXeT__5224328E");
        });

        modelBuilder.Entity<ChucVu>(entity =>
        {
            entity.HasKey(e => e.MaChucVu).HasName("PK__ChucVu__D4639533B4A2E195");

            entity.ToTable("ChucVu", tb => tb.HasTrigger("TR_ChucVu_GiuQuanLy"));

            entity.HasIndex(e => e.TenChucVu, "UQ__ChucVu__A7E2123E5EEC7E6C").IsUnique();

            entity.Property(e => e.LuongCoBan).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.MoTa).HasMaxLength(500);
            entity.Property(e => e.TenChucVu).HasMaxLength(100);
        });

        modelBuilder.Entity<DanhGia>(entity =>
        {
            entity.HasKey(e => e.MaDanhGia).HasName("PK__DanhGia__AA9515BF64EF834D");

            entity.HasIndex(e => new { e.MaKh, e.MaXe }, "UQ__DanhGia__E5579D131C42579E").IsUnique();

            entity.HasIndex(e => e.MaCt, "UX_DanhGia_MaCT")
                .IsUnique()
                .HasFilter("([MaCT] IS NOT NULL)");

            entity.Property(e => e.MaCt).HasColumnName("MaCT");
            entity.Property(e => e.MaKh).HasColumnName("MaKH");
            entity.Property(e => e.NgayDanhGia)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NoiDung).HasMaxLength(1000);

            entity.HasOne(d => d.MaCtNavigation).WithOne(p => p.DanhGium)
                .HasForeignKey<DanhGia>(d => d.MaCt)
                .HasConstraintName("FK__DanhGia__MaCT__72910220");

            entity.HasOne(d => d.MaKhNavigation).WithMany(p => p.DanhGia)
                .HasForeignKey(d => d.MaKh)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DanhGia__MaKH__70A8B9AE");

            entity.HasOne(d => d.MaXeNavigation).WithMany(p => p.DanhGia)
                .HasForeignKey(d => d.MaXe)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DanhGia__MaXe__719CDDE7");
        });

        modelBuilder.Entity<DonHang>(entity =>
        {
            entity.HasKey(e => e.MaDonHang).HasName("PK__DonHang__129584AD0674EECD");

            entity.ToTable("DonHang");

            entity.Property(e => e.DiaChiGiao).HasMaxLength(255);
            entity.Property(e => e.GhiChu).HasMaxLength(500);
            entity.Property(e => e.GiamGia).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.MaKh).HasColumnName("MaKH");
            entity.Property(e => e.MaKm).HasColumnName("MaKM");
            entity.Property(e => e.MaNv).HasColumnName("MaNV");
            entity.Property(e => e.NgayDat)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PhiVanChuyen).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.SdtnguoiNhan)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("SDTNguoiNhan");
            entity.Property(e => e.TenNguoiNhan).HasMaxLength(100);
            entity.Property(e => e.TongTien).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(30)
                .HasDefaultValue("ChoXacNhan");

            entity.HasOne(d => d.MaKhNavigation).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.MaKh)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DonHang__MaKH__282DF8C2");

            entity.HasOne(d => d.MaKmNavigation).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.MaKm)
                .HasConstraintName("FK__DonHang__MaKM__29221CFB");

            entity.HasOne(d => d.MaNvNavigation).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.MaNv)
                .HasConstraintName("FK__DonHang__MaNV__2A164134");
        });

        modelBuilder.Entity<GoiBaoHanh>(entity =>
        {
            entity.HasKey(e => e.MaGoiBh).HasName("PK__GoiBaoHa__6EC6D01005F1B393");

            entity.ToTable("GoiBaoHanh");

            entity.Property(e => e.MaGoiBh).HasColumnName("MaGoiBH");
            entity.Property(e => e.GiaTien).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.MoTa).HasMaxLength(500);
            entity.Property(e => e.TenGoi).HasMaxLength(100);
        });

        modelBuilder.Entity<HangXe>(entity =>
        {
            entity.HasKey(e => e.MaHang).HasName("PK__HangXe__19C0DB1DFD704A00");

            entity.ToTable("HangXe");

            entity.HasIndex(e => e.TenHang, "UQ__HangXe__E6C9BFA866B6171A").IsUnique();

            entity.Property(e => e.Logo).HasMaxLength(255);
            entity.Property(e => e.MoTa).HasMaxLength(500);
            entity.Property(e => e.QuocGia).HasMaxLength(100);
            entity.Property(e => e.TenHang).HasMaxLength(100);
        });

        modelBuilder.Entity<HoaDon>(entity =>
        {
            entity.HasKey(e => e.MaHd).HasName("PK__HoaDon__2725A6E0C78EF84B");

            entity.ToTable("HoaDon", tb => tb.HasTrigger("TR_HoaDon_Insert"));

            entity.HasIndex(e => e.SoHoaDon, "UQ__HoaDon__012E9E53F5FC7C56").IsUnique();

            entity.HasIndex(e => e.MaDonHang, "UQ__HoaDon__129584ACC9F50FF8").IsUnique();

            entity.Property(e => e.MaHd).HasColumnName("MaHD");
            entity.Property(e => e.MaNv).HasColumnName("MaNV");
            entity.Property(e => e.NgayLap)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SoHoaDon)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.TongTien).HasColumnType("decimal(18, 0)");

            entity.HasOne(d => d.MaDonHangNavigation).WithOne(p => p.HoaDon)
                .HasForeignKey<HoaDon>(d => d.MaDonHang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__HoaDon__MaDonHan__498EEC8D");

            entity.HasOne(d => d.MaNvNavigation).WithMany(p => p.HoaDons)
                .HasForeignKey(d => d.MaNv)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__HoaDon__MaNV__4A8310C6");
        });

        modelBuilder.Entity<KhachHang>(entity =>
        {
            entity.HasKey(e => e.MaKh).HasName("PK__KhachHan__2725CF1E27D46D68");

            entity.ToTable("KhachHang");

            entity.HasIndex(e => e.Email, "UQ__KhachHan__A9D10534BAC52C28").IsUnique();

            entity.Property(e => e.MaKh).HasColumnName("MaKH");
            entity.Property(e => e.DiaChi).HasMaxLength(255);
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.NgayDangKy)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(15)
                .IsUnicode(false);
        });

        modelBuilder.Entity<KhuyenMai>(entity =>
        {
            entity.HasKey(e => e.MaKm).HasName("PK__KhuyenMa__2725CF1531A3A10C");

            entity.ToTable("KhuyenMai");

            entity.HasIndex(e => e.MaCode, "UQ__KhuyenMa__152C7C5CC0A16212").IsUnique();

            entity.Property(e => e.MaKm).HasColumnName("MaKM");
            entity.Property(e => e.GiaTri).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.LoaiGiam).HasMaxLength(20);
            entity.Property(e => e.MaCode)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.TrangThai).HasDefaultValue(true);
        });

        modelBuilder.Entity<LichLaiThu>(entity =>
        {
            entity.HasKey(e => e.MaLich).HasName("PK__LichLaiT__728A9AE94979B2C6");

            entity.ToTable("LichLaiThu");

            entity.Property(e => e.GhiChu).HasMaxLength(500);
            entity.Property(e => e.MaKh).HasColumnName("MaKH");
            entity.Property(e => e.NgayHen).HasColumnType("datetime");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(30)
                .HasDefaultValue("ChoXacNhan");

            entity.HasOne(d => d.MaKhNavigation).WithMany(p => p.LichLaiThus)
                .HasForeignKey(d => d.MaKh)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LichLaiThu__MaKH__7755B73D");

            entity.HasOne(d => d.MaXeNavigation).WithMany(p => p.LichLaiThus)
                .HasForeignKey(d => d.MaXe)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LichLaiThu__MaXe__7849DB76");
        });

        modelBuilder.Entity<LichSuDonHang>(entity =>
        {
            entity.HasKey(e => e.MaLs).HasName("PK__LichSuDo__2725C772FA10A512");

            entity.ToTable("LichSuDonHang");

            entity.Property(e => e.MaLs).HasColumnName("MaLS");
            entity.Property(e => e.GhiChu).HasMaxLength(500);
            entity.Property(e => e.MaNv).HasColumnName("MaNV");
            entity.Property(e => e.ThoiGian)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TrangThai).HasMaxLength(30);

            entity.HasOne(d => d.MaDonHangNavigation).WithMany(p => p.LichSuDonHangs)
                .HasForeignKey(d => d.MaDonHang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LichSuDon__MaDon__3B40CD36");

            entity.HasOne(d => d.MaNvNavigation).WithMany(p => p.LichSuDonHangs)
                .HasForeignKey(d => d.MaNv)
                .HasConstraintName("FK__LichSuDonH__MaNV__3C34F16F");
        });

        modelBuilder.Entity<LoaiXe>(entity =>
        {
            entity.HasKey(e => e.MaLoai).HasName("PK__LoaiXe__730A5759F0AF1613");

            entity.ToTable("LoaiXe");

            entity.HasIndex(e => e.TenLoai, "UQ__LoaiXe__E29B1042DAE7D44A").IsUnique();

            entity.Property(e => e.MoTa).HasMaxLength(500);
            entity.Property(e => e.TenLoai).HasMaxLength(100);
        });

        modelBuilder.Entity<MauXe>(entity =>
        {
            entity.HasKey(e => e.MaMau).HasName("PK__MauXe__3A5BBB7D4F09C60C");

            entity.ToTable("MauXe");

            entity.HasIndex(e => new { e.MaXe, e.TenMau }, "UQ__MauXe__3417D665DF650A86").IsUnique();

            entity.HasIndex(e => new { e.MaMau, e.MaXe }, "UQ__MauXe__F829E9705EC0513E").IsUnique();

            entity.Property(e => e.MaHex)
                .HasMaxLength(7)
                .IsUnicode(false);
            entity.Property(e => e.TenMau).HasMaxLength(50);

            entity.HasOne(d => d.MaXeNavigation).WithMany(p => p.MauXes)
                .HasForeignKey(d => d.MaXe)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MauXe__MaXe__5CD6CB2B");
        });

        modelBuilder.Entity<NhaCungCap>(entity =>
        {
            entity.HasKey(e => e.MaNcc).HasName("PK__NhaCungC__3A185DEB51EC9350");

            entity.ToTable("NhaCungCap");

            entity.HasIndex(e => e.TenNcc, "UQ__NhaCungC__CCE06C524F16BF6C").IsUnique();

            entity.Property(e => e.MaNcc).HasColumnName("MaNCC");
            entity.Property(e => e.DiaChi).HasMaxLength(255);
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.TenNcc)
                .HasMaxLength(150)
                .HasColumnName("TenNCC");
            entity.Property(e => e.TrangThai).HasDefaultValue(true);
        });

        modelBuilder.Entity<NhanVien>(entity =>
        {
            entity.HasKey(e => e.MaNv).HasName("PK__NhanVien__2725D70A406F0E21");

            entity.ToTable("NhanVien", tb => tb.HasTrigger("TR_NhanVien_GiuQuanLy"));

            entity.HasIndex(e => e.Email, "UQ__NhanVien__A9D10534E7E213BB").IsUnique();

            entity.Property(e => e.MaNv).HasColumnName("MaNV");
            entity.Property(e => e.DiaChi).HasMaxLength(255);
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.HeSoLuong)
                .HasDefaultValue(1m)
                .HasColumnType("decimal(4, 2)");
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.NgayVaoLam).HasDefaultValueSql("(CONVERT([date],getdate()))");
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasDefaultValue("DangLam");

            entity.HasOne(d => d.MaChucVuNavigation).WithMany(p => p.NhanViens)
                .HasForeignKey(d => d.MaChucVu)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__NhanVien__MaChuc__6B24EA82");

            entity.HasOne(d => d.MaQuanLyNavigation).WithMany(p => p.InverseMaQuanLyNavigation)
                .HasForeignKey(d => d.MaQuanLy)
                .HasConstraintName("FK__NhanVien__MaQuan__6C190EBB");
        });

        modelBuilder.Entity<NhanVienQuyen>(entity =>
        {
            entity.HasKey(e => new { e.MaNv, e.MaQuyen }).HasName("PK__NhanVien__76F160E7A0C69577");

            entity.ToTable("NhanVienQuyen", tb => tb.HasTrigger("TR_NhanVienQuyen_NguoiCap"));

            entity.Property(e => e.MaNv).HasColumnName("MaNV");
            entity.Property(e => e.NgayCap)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaNguoiCapNavigation).WithMany(p => p.NhanVienQuyenMaNguoiCapNavigations)
                .HasForeignKey(d => d.MaNguoiCap)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__NhanVienQ__MaNgu__02084FDA");

            entity.HasOne(d => d.MaNvNavigation).WithMany(p => p.NhanVienQuyenMaNvNavigations)
                .HasForeignKey(d => d.MaNv)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__NhanVienQu__MaNV__00200768");

            entity.HasOne(d => d.MaQuyenNavigation).WithMany(p => p.NhanVienQuyens)
                .HasForeignKey(d => d.MaQuyen)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__NhanVienQ__MaQuy__01142BA1");
        });

        modelBuilder.Entity<NhatKyHeThong>(entity =>
        {
            entity.HasKey(e => e.MaNk).HasName("PK__NhatKyHe__2725D73F46B2DBF0");

            entity.ToTable("NhatKyHeThong");

            entity.Property(e => e.MaNk).HasColumnName("MaNK");
            entity.Property(e => e.BangTacDong).HasMaxLength(50);
            entity.Property(e => e.ChiTiet).HasMaxLength(1000);
            entity.Property(e => e.HanhDong).HasMaxLength(50);
            entity.Property(e => e.MaNv).HasColumnName("MaNV");
            entity.Property(e => e.ThoiGian)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaNvNavigation).WithMany(p => p.NhatKyHeThongs)
                .HasForeignKey(d => d.MaNv)
                .HasConstraintName("FK__NhatKyHeTh__MaNV__7D0E9093");
        });

        modelBuilder.Entity<PhieuBaoHanh>(entity =>
        {
            entity.HasKey(e => e.MaPbh).HasName("PK__PhieuBao__3AE079057E987775");

            entity.ToTable("PhieuBaoHanh");

            entity.Property(e => e.MaPbh).HasColumnName("MaPBH");
            entity.Property(e => e.ChiPhi).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.KetQuaXuLy).HasMaxLength(500);
            entity.Property(e => e.MaBh).HasColumnName("MaBH");
            entity.Property(e => e.MaNv).HasColumnName("MaNV");
            entity.Property(e => e.NgayHoanThanh).HasColumnType("datetime");
            entity.Property(e => e.NgayTiepNhan)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NoiDungLoi).HasMaxLength(500);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(30)
                .HasDefaultValue("DaTiepNhan");

            entity.HasOne(d => d.MaBhNavigation).WithMany(p => p.PhieuBaoHanhs)
                .HasForeignKey(d => d.MaBh)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PhieuBaoHa__MaBH__625A9A57");

            entity.HasOne(d => d.MaNvNavigation).WithMany(p => p.PhieuBaoHanhs)
                .HasForeignKey(d => d.MaNv)
                .HasConstraintName("FK__PhieuBaoHa__MaNV__634EBE90");
        });

        modelBuilder.Entity<PhieuNhap>(entity =>
        {
            entity.HasKey(e => e.MaPn).HasName("PK__PhieuNha__2725E7F065EC982C");

            entity.ToTable("PhieuNhap");

            entity.Property(e => e.MaPn).HasColumnName("MaPN");
            entity.Property(e => e.GhiChu).HasMaxLength(500);
            entity.Property(e => e.MaNcc).HasColumnName("MaNCC");
            entity.Property(e => e.MaNv).HasColumnName("MaNV");
            entity.Property(e => e.NgayNhap).HasDefaultValueSql("(CONVERT([date],getdate()))");

            entity.HasOne(d => d.MaNccNavigation).WithMany(p => p.PhieuNhaps)
                .HasForeignKey(d => d.MaNcc)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PhieuNhap__MaNCC__14270015");

            entity.HasOne(d => d.MaNvNavigation).WithMany(p => p.PhieuNhaps)
                .HasForeignKey(d => d.MaNv)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PhieuNhap__MaNV__151B244E");
        });

        modelBuilder.Entity<Quyen>(entity =>
        {
            entity.HasKey(e => e.MaQuyen).HasName("PK__Quyen__1D4B7ED45E2671C5");

            entity.ToTable("Quyen");

            entity.HasIndex(e => e.KyHieu, "UQ__Quyen__1AC4F8FA3AE16AFC").IsUnique();

            entity.Property(e => e.KyHieu)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MoTa).HasMaxLength(255);
            entity.Property(e => e.NhomChucNang).HasMaxLength(50);
            entity.Property(e => e.TenQuyen).HasMaxLength(100);
        });

        modelBuilder.Entity<TaiKhoanKhachHang>(entity =>
        {
            entity.HasKey(e => e.MaTkkh).HasName("PK__TaiKhoan__B50CD23269D40CB2");

            entity.ToTable("TaiKhoanKhachHang");

            entity.HasIndex(e => e.MaKh, "UQ__TaiKhoan__2725CF1F0C5292C2").IsUnique();

            entity.HasIndex(e => e.TenDangNhap, "UQ__TaiKhoan__55F68FC0D30F5A3C").IsUnique();

            entity.Property(e => e.MaTkkh).HasColumnName("MaTKKH");
            entity.Property(e => e.MaKh).HasColumnName("MaKH");
            entity.Property(e => e.MatKhauHash)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TenDangNhap)
                .HasMaxLength(50);
            entity.Property(e => e.TrangThai).HasDefaultValue(true);

            entity.HasOne(d => d.MaKhNavigation).WithOne(p => p.TaiKhoanKhachHang)
                .HasForeignKey<TaiKhoanKhachHang>(d => d.MaKh)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TaiKhoanKh__MaKH__0B91BA14");
        });

        modelBuilder.Entity<TaiKhoanNhanVien>(entity =>
        {
            entity.HasKey(e => e.MaTknv).HasName("PK__TaiKhoan__B50CFB8EA0D06350");

            entity.ToTable("TaiKhoanNhanVien", tb =>
            {
                tb.HasTrigger("TR_TaiKhoanNhanVien_GiuQuanLy");
                tb.HasTrigger("TR_TaiKhoanNhanVien_NguoiCap");
            });

            entity.HasIndex(e => e.MaNv, "UQ__TaiKhoan__2725D70BB4E6ED3A").IsUnique();

            entity.HasIndex(e => e.TenDangNhap, "UQ__TaiKhoan__55F68FC065309211").IsUnique();

            entity.Property(e => e.MaTknv).HasColumnName("MaTKNV");
            entity.Property(e => e.BatBuocDoiMatKhau).HasDefaultValue(true);
            entity.Property(e => e.MaNv).HasColumnName("MaNV");
            entity.Property(e => e.MatKhauHash)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.NgayCap)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NgayDoiMatKhau).HasColumnType("datetime");
            entity.Property(e => e.TenDangNhap)
                .HasMaxLength(50);
            entity.Property(e => e.TrangThai).HasDefaultValue(true);

            entity.HasOne(d => d.MaNguoiCapNavigation).WithMany(p => p.TaiKhoanNhanVienMaNguoiCapNavigations)
                .HasForeignKey(d => d.MaNguoiCap)
                .HasConstraintName("FK__TaiKhoanN__MaNgu__778AC167");

            entity.HasOne(d => d.MaNvNavigation).WithOne(p => p.TaiKhoanNhanVienMaNvNavigation)
                .HasForeignKey<TaiKhoanNhanVien>(d => d.MaNv)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TaiKhoanNh__MaNV__76969D2E");
        });

        modelBuilder.Entity<ThanhToan>(entity =>
        {
            entity.HasKey(e => e.MaThanhToan).HasName("PK__ThanhToa__D4B258441BE16388");

            entity.ToTable("ThanhToan");

            entity.Property(e => e.LoaiThanhToan).HasMaxLength(30);
            entity.Property(e => e.MaGiaoDich)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.NgayThanhToan)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PhuongThuc).HasMaxLength(50);
            entity.Property(e => e.SoTien).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(30)
                .HasDefaultValue("ThanhCong");

            entity.HasOne(d => d.MaDonHangNavigation).WithMany(p => p.ThanhToans)
                .HasForeignKey(d => d.MaDonHang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ThanhToan__MaDon__40058253");
        });

        modelBuilder.Entity<Xe>(entity =>
        {
            entity.HasKey(e => e.MaXe).HasName("PK__Xe__272520CDF7147584");

            entity.ToTable("Xe");

            entity.Property(e => e.CongSuatKw)
                .HasColumnType("decimal(6, 1)")
                .HasColumnName("CongSuatKW");
            entity.Property(e => e.DungTichCc).HasColumnName("DungTichCC");
            entity.Property(e => e.GiaNiemYet).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.HienThi).HasDefaultValue(true);
            entity.Property(e => e.HopSo).HasMaxLength(50);
            entity.Property(e => e.KieuDanDong).HasMaxLength(50);
            entity.Property(e => e.MoTa).HasMaxLength(500);
            entity.Property(e => e.SoCho).HasDefaultValue(2);
            entity.Property(e => e.TenXe).HasMaxLength(200);

            entity.HasOne(d => d.MaHangNavigation).WithMany(p => p.Xes)
                .HasForeignKey(d => d.MaHang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Xe__MaHang__5070F446");

            entity.HasOne(d => d.MaLoaiNavigation).WithMany(p => p.Xes)
                .HasForeignKey(d => d.MaLoai)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Xe__MaLoai__5165187F");
        });

        modelBuilder.Entity<XeThucTe>(entity =>
        {
            entity.HasKey(e => e.MaXeThucTe).HasName("PK__XeThucTe__175A504A2275F9E7");

            entity.ToTable("XeThucTe");

            entity.HasIndex(e => e.SoMay, "UQ__XeThucTe__2002CBFDCC823421").IsUnique();

            entity.HasIndex(e => e.SoKhung, "UQ__XeThucTe__D8B5889AD0F2059F").IsUnique();

            entity.HasIndex(e => e.BienSo, "UX_XeThucTe_BienSo")
                .IsUnique()
                .HasFilter("([BienSo] IS NOT NULL)");

            entity.Property(e => e.BienSo)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.GiaNhap).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.MaPn).HasColumnName("MaPN");
            entity.Property(e => e.SoKhung)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.SoMay)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasDefaultValue("ConHang");

            entity.HasOne(d => d.MaPnNavigation).WithMany(p => p.XeThucTes)
                .HasForeignKey(d => d.MaPn)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__XeThucTe__MaPN__1AD3FDA4");

            entity.HasOne(d => d.MauXe).WithMany(p => p.XeThucTes)
                .HasPrincipalKey(p => new { p.MaMau, p.MaXe })
                .HasForeignKey(d => new { d.MaMau, d.MaXe })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__XeThucTe__1EA48E88");
        });

        modelBuilder.Entity<YeuThich>(entity =>
        {
            entity.HasKey(e => new { e.MaKh, e.MaXe }).HasName("PK__YeuThich__E5579D1264C0E0A9");

            entity.ToTable("YeuThich");

            entity.Property(e => e.MaKh).HasColumnName("MaKH");
            entity.Property(e => e.NgayThem)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaKhNavigation).WithMany(p => p.YeuThiches)
                .HasForeignKey(d => d.MaKh)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__YeuThich__MaKH__6AEFE058");

            entity.HasOne(d => d.MaXeNavigation).WithMany(p => p.YeuThiches)
                .HasForeignKey(d => d.MaXe)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__YeuThich__MaXe__6BE40491");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
