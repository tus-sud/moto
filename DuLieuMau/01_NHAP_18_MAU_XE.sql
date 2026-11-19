-- MotoShopABG: 18 mẫu xe, lấy nguồn hãng ngày 03/11/2026.
-- Trước khi Execute, CHỌN DATABASE ĐANG DÙNG CỦA WEB trong ô database của SSMS.
-- Chỉ thêm mẫu xe chưa có; không UPDATE/DELETE xe cũ; không thêm tồn kho hoặc đơn hàng.
SET NOCOUNT ON;
SET XACT_ABORT ON;
IF DB_NAME() IN (N'master', N'model', N'msdb', N'tempdb')
    THROW 50000, N'Hãy chọn database của web trong thanh công cụ SSMS trước khi chạy.', 1;
IF OBJECT_ID(N'dbo.Xe',N'U') IS NULL OR OBJECT_ID(N'dbo.HangXe',N'U') IS NULL
   OR OBJECT_ID(N'dbo.LoaiXe',N'U') IS NULL OR OBJECT_ID(N'dbo.AnhXe',N'U') IS NULL
   OR OBJECT_ID(N'dbo.MauXe',N'U') IS NULL OR COL_LENGTH(N'dbo.Xe',N'GiaNiemYet') IS NULL
    THROW 50001, N'Database không khớp cấu trúc MotoShopWeb. Không có dữ liệu nào được nhập.', 1;

DECLARE @Seed TABLE (
    TenHang NVARCHAR(100), TenLoai NVARCHAR(100), TenXe NVARCHAR(200) PRIMARY KEY,
    DungTichCC INT, CongSuatKW DECIMAL(6,1), HopSo NVARCHAR(50),
    GiaNiemYet DECIMAL(18,0), MoTa NVARCHAR(500));
INSERT INTO @Seed VALUES
(N'Honda', N'Touring', N'Honda Gold Wing 2025', 1833, 93.0, N'Ly hợp kép 7 cấp DCT', 1231500000, N'[ABG-DEMO-20261103] Touring; dung tích 1833 cc. Yên: 745 mm; Khối lượng: 390 kg; Bình xăng: 21 lít. Giá đề xuất hãng để tham khảo/demo, không phải xác nhận tồn kho hay giá chốt của MotoShopABG.'),
(N'Honda', N'Sport', N'Honda CBR1000RR-R Fireblade SP 2024', 1000, 140.0, N'Côn tay 6 số', 1051000000, N'[ABG-DEMO-20261103] Sport; dung tích 1000 cc. Yên: 830 mm; Khối lượng: 201 kg; Bình xăng: 16,5 lít. Giá đề xuất hãng để tham khảo/demo, không phải xác nhận tồn kho hay giá chốt của MotoShopABG.'),
(N'Honda', N'Adventure', N'Honda Africa Twin Adventure Sports 2026', 1084, 75.0, N'Hộp số tự động ly hợp kép 6 cấp DCT', 620990000, N'[ABG-DEMO-20261103] Adventure; dung tích 1084 cc. Yên: 835-855 mm; Khối lượng: 253 kg; Bình xăng: 24,8 lít. Giá đề xuất hãng để tham khảo/demo, không phải xác nhận tồn kho hay giá chốt của MotoShopABG.'),
(N'Honda', N'Adventure', N'Honda Africa Twin Tiêu chuẩn 2026', 1084, 75.0, N'Côn tay 6 số', 540990000, N'[ABG-DEMO-20261103] Adventure; dung tích 1084 cc. Yên: 850-870 mm; Khối lượng: 233 kg; Bình xăng: 18,8 lít. Giá đề xuất hãng để tham khảo/demo, không phải xác nhận tồn kho hay giá chốt của MotoShopABG.'),
(N'Honda', N'Cruiser', N'Honda Rebel 1100 2025', 1084, 65.0, N'Hộp số ly hợp kép 6 cấp DCT', 399990000, N'[ABG-DEMO-20261103] Cruiser; dung tích 1084 cc. Yên: 710 mm; Khối lượng: 236 kg; Bình xăng: 13,6 lít. Giá đề xuất hãng để tham khảo/demo, không phải xác nhận tồn kho hay giá chốt của MotoShopABG.'),
(N'Honda', N'Naked', N'Honda CB1000 Hornet', 1000, 84.0, N'Côn tay 6 số', 339900000, N'[ABG-DEMO-20261103] Naked; dung tích 1000 cc. Yên: 809 mm; Khối lượng: 211 kg; Bình xăng: 17 lít. Giá đề xuất hãng để tham khảo/demo, không phải xác nhận tồn kho hay giá chốt của MotoShopABG.'),
(N'Honda', N'Adventure', N'Honda Transalp 2025', 755, 67.0, N'Côn tay 6 số', 299990000, N'[ABG-DEMO-20261103] Adventure; dung tích 755 cc. Yên: 850 mm; Khối lượng: 210 kg; Bình xăng: 16,9 lít. Giá đề xuất hãng để tham khảo/demo, không phải xác nhận tồn kho hay giá chốt của MotoShopABG.'),
(N'Honda', N'Classic', N'Honda GB350S', 348, 15.5, N'5 cấp', 134990000, N'[ABG-DEMO-20261103] Classic; dung tích 348 cc. Yên: 800 mm; Khối lượng: 178 kg; Bình xăng: 15,0 lít. Giá đề xuất hãng để tham khảo/demo, không phải xác nhận tồn kho hay giá chốt của MotoShopABG.'),
(N'Honda', N'Cruiser', N'Honda Rebel 500 2025', 471, 33.6, N'Côn tay 6 số', 181300000, N'[ABG-DEMO-20261103] Cruiser; dung tích 471 cc. Yên: 690 mm; Khối lượng: 190 kg; Bình xăng: 11,2 Lít. Giá đề xuất hãng để tham khảo/demo, không phải xác nhận tồn kho hay giá chốt của MotoShopABG.'),
(N'Honda', N'Scrambler', N'Honda CL500', 471, 34.0, N'Côn tay 6 số', 180990000, N'[ABG-DEMO-20261103] Scrambler; dung tích 471 cc. Yên: 790 mm; Khối lượng: 193 kg; Bình xăng: 12,2 Lít. Giá đề xuất hãng để tham khảo/demo, không phải xác nhận tồn kho hay giá chốt của MotoShopABG.'),
(N'Honda', N'Sport', N'Honda CBR650R 2026', 649, 61.7, N'Côn tay 6 số', 269990000, N'[ABG-DEMO-20261103] Sport; dung tích 649 cc. Yên: 810 mm; Khối lượng: 210 kg; Bình xăng: 15,4 lít. Giá đề xuất hãng để tham khảo/demo, không phải xác nhận tồn kho hay giá chốt của MotoShopABG.'),
(N'Honda', N'Naked', N'Honda CB650R 2026', 649, 61.7, N'Côn tay 6 số', 261990000, N'[ABG-DEMO-20261103] Naked; dung tích 649 cc. Yên: 810 mm; Khối lượng: 205 kg; Bình xăng: 15,4 lít. Giá đề xuất hãng để tham khảo/demo, không phải xác nhận tồn kho hay giá chốt của MotoShopABG.'),
(N'Honda', N'Sport', N'Honda CBR500R 2026', 471, 37.1, N'Côn tay 6 số', 209990000, N'[ABG-DEMO-20261103] Sport; dung tích 471 cc. Yên: 785 mm; Khối lượng: 195 kg; Bình xăng: 17,1 lít. Giá đề xuất hãng để tham khảo/demo, không phải xác nhận tồn kho hay giá chốt của MotoShopABG.'),
(N'Honda', N'Naked', N'Honda CB500 Hornet 2026', 471, 37.1, N'Côn tay 6 số', 199990000, N'[ABG-DEMO-20261103] Naked; dung tích 471 cc. Yên: 785 mm; Khối lượng: 192 kg; Bình xăng: 17,1 lít. Giá đề xuất hãng để tham khảo/demo, không phải xác nhận tồn kho hay giá chốt của MotoShopABG.'),
(N'Honda', N'Adventure', N'Honda NX500 2026', 471, 37.1, N'Côn tay 6 số', 213290000, N'[ABG-DEMO-20261103] Adventure; dung tích 471 cc. Yên: 830 mm; Khối lượng: 200 kg; Bình xăng: 17,7 lít. Giá đề xuất hãng để tham khảo/demo, không phải xác nhận tồn kho hay giá chốt của MotoShopABG.'),
(N'Kawasaki', N'Naked', N'Kawasaki Z650 2026', 649, 50.2, N'6 cấp', 199000000, N'[ABG-DEMO-20261103] Naked; dung tích 649 cc. Yên: 805 mm; Khối lượng: 190 kg; Bình xăng: 15 lít. Giá đề xuất hãng để tham khảo/demo, không phải xác nhận tồn kho hay giá chốt của MotoShopABG.'),
(N'Kawasaki', N'Naked', N'Kawasaki Z900 2026', 948, 91.0, N'Hộp số 6 cấp', 299000000, N'[ABG-DEMO-20261103] Naked; dung tích 948 cc. Yên: 830 mm; Khối lượng: 213 kg; Bình xăng: 17 lít. Giá đề xuất hãng để tham khảo/demo, không phải xác nhận tồn kho hay giá chốt của MotoShopABG.'),
(N'Kawasaki', N'Sport', N'Kawasaki Ninja ZX-4RR 2026', 399, 57.0, N'Hộp số 6 cấp', 229000000, N'[ABG-DEMO-20261103] Sport; dung tích 399 cc. Yên: 800 mm; Khối lượng: 189 kg; Bình xăng: 15 lít. Giá đề xuất hãng để tham khảo/demo, không phải xác nhận tồn kho hay giá chốt của MotoShopABG.');
DECLARE @Colors TABLE (TenXe NVARCHAR(200), TenMau NVARCHAR(50));
INSERT INTO @Colors VALUES
(N'Honda Gold Wing 2025', N'Vàng Bạc Đen'),
(N'Honda Gold Wing 2025', N'Đỏ Đen'),
(N'Honda CBR1000RR-R Fireblade SP 2024', N'Đỏ xanh trắng'),
(N'Honda Africa Twin Adventure Sports 2026', N'Trắng Xanh Đỏ'),
(N'Honda Africa Twin Tiêu chuẩn 2026', N'Đỏ Đen'),
(N'Honda Rebel 1100 2025', N'Đen'),
(N'Honda CB1000 Hornet', N'Đen'),
(N'Honda Transalp 2025', N'Trắng Đen'),
(N'Honda Transalp 2025', N'Đen'),
(N'Honda GB350S', N'Xám Đen'),
(N'Honda GB350S', N'Bạc Đen'),
(N'Honda Rebel 500 2025', N'Đen'),
(N'Honda Rebel 500 2025', N'Trắng'),
(N'Honda Rebel 500 2025', N'Xám'),
(N'Honda CL500', N'Đen'),
(N'Honda CL500', N'Nâu đen'),
(N'Honda CBR650R 2026', N'Đỏ'),
(N'Honda CBR650R 2026', N'Đen'),
(N'Honda CB650R 2026', N'Đen Đỏ'),
(N'Honda CB650R 2026', N'Đen'),
(N'Honda CB650R 2026', N'Xanh Đen'),
(N'Honda CBR500R 2026', N'Đỏ'),
(N'Honda CB500 Hornet 2026', N'Đen'),
(N'Honda CB500 Hornet 2026', N'Đỏ đen'),
(N'Honda NX500 2026', N'Trắng Bạc Đen'),
(N'Honda NX500 2026', N'Đen'),
(N'Honda NX500 2026', N'Đen Xanh'),
(N'Kawasaki Ninja ZX-4RR 2026', N'Xanh Lime');
DECLARE @Photos TABLE (TenXe NVARCHAR(200), DuongDan NVARCHAR(255), ThuTu INT);
INSERT INTO @Photos VALUES
(N'Honda Gold Wing 2025', N'/images/catalog-abg/gold-wing-2025-1.png', 0),
(N'Honda Gold Wing 2025', N'/images/catalog-abg/gold-wing-2025-2.png', 1),
(N'Honda CBR1000RR-R Fireblade SP 2024', N'/images/catalog-abg/cbr1000rr-r-fireblade-sp-2024-1.png', 0),
(N'Honda Africa Twin Adventure Sports 2026', N'/images/catalog-abg/africa-twin-2026-ban-adventure-sports-1.png', 0),
(N'Honda Africa Twin Tiêu chuẩn 2026', N'/images/catalog-abg/africa-twin-2026-ban-tieu-chuan-1.png', 0),
(N'Honda Rebel 1100 2025', N'/images/catalog-abg/rebel-1100-2025-1.png', 0),
(N'Honda CB1000 Hornet', N'/images/catalog-abg/cb1000-hornet-1.jpg', 0),
(N'Honda Transalp 2025', N'/images/catalog-abg/transalp-2025-1.png', 0),
(N'Honda Transalp 2025', N'/images/catalog-abg/transalp-2025-2.png', 1),
(N'Honda GB350S', N'/images/catalog-abg/gb350s-1.png', 0),
(N'Honda GB350S', N'/images/catalog-abg/gb350s-2.png', 1),
(N'Honda Rebel 500 2025', N'/images/catalog-abg/rebel-500-2025-1.png', 0),
(N'Honda Rebel 500 2025', N'/images/catalog-abg/rebel-500-2025-2.png', 1),
(N'Honda CL500', N'/images/catalog-abg/cl500-1.jpg', 0),
(N'Honda CL500', N'/images/catalog-abg/cl500-2.jpg', 1),
(N'Honda CBR650R 2026', N'/images/catalog-abg/cbr650r-2026-1.png', 0),
(N'Honda CBR650R 2026', N'/images/catalog-abg/cbr650r-2026-2.png', 1),
(N'Honda CB650R 2026', N'/images/catalog-abg/cb650r-2026-1.png', 0),
(N'Honda CB650R 2026', N'/images/catalog-abg/cb650r-2026-2.png', 1),
(N'Honda CBR500R 2026', N'/images/catalog-abg/cbr500r-2026-1.png', 0),
(N'Honda CB500 Hornet 2026', N'/images/catalog-abg/cb500-hornet-2026-1.png', 0),
(N'Honda CB500 Hornet 2026', N'/images/catalog-abg/cb500-hornet-2026-2.png', 1),
(N'Honda NX500 2026', N'/images/catalog-abg/nx500-2026-1.jpg', 0),
(N'Honda NX500 2026', N'/images/catalog-abg/nx500-2026-2.jpg', 1),
(N'Kawasaki Z650 2026', N'/images/catalog-abg/2026-z650-s-1.jpg', 0),
(N'Kawasaki Z650 2026', N'/images/catalog-abg/2026-z650-s-2.jpg', 1),
(N'Kawasaki Z900 2026', N'/images/catalog-abg/2026-z900-1.jpg', 0),
(N'Kawasaki Z900 2026', N'/images/catalog-abg/2026-z900-2.jpg', 1),
(N'Kawasaki Ninja ZX-4RR 2026', N'/images/catalog-abg/2026-ninja-zx-4rr-1.png', 0),
(N'Kawasaki Ninja ZX-4RR 2026', N'/images/catalog-abg/2026-ninja-zx-4rr-2.jpg', 1);
DECLARE @Inserted TABLE (MaXe INT, TenXe NVARCHAR(200));
BEGIN TRY
    BEGIN TRANSACTION;
    INSERT dbo.HangXe (TenHang, QuocGia, MoTa)
    SELECT DISTINCT s.TenHang, N'Nhật Bản', N'Hãng xe trong bộ dữ liệu tham khảo MotoShopABG.'
    FROM @Seed s
    WHERE NOT EXISTS (SELECT 1 FROM dbo.HangXe h WITH (UPDLOCK,HOLDLOCK) WHERE h.TenHang=s.TenHang);

    INSERT dbo.LoaiXe (TenLoai, MoTa)
    SELECT DISTINCT s.TenLoai, N'Phân loại kiểu dáng phục vụ danh mục xe.'
    FROM @Seed s
    WHERE NOT EXISTS (SELECT 1 FROM dbo.LoaiXe l WITH (UPDLOCK,HOLDLOCK) WHERE l.TenLoai=s.TenLoai);

    INSERT dbo.Xe (MaHang, MaLoai, TenXe, NamSanXuat, GiaNiemYet, DungTichCC, CongSuatKW, HopSo, KieuDanDong, SoCho, MoTa, HienThi)
    OUTPUT inserted.MaXe, inserted.TenXe INTO @Inserted
    SELECT h.MaHang, l.MaLoai, s.TenXe, NULL, s.GiaNiemYet, s.DungTichCC, s.CongSuatKW,
           s.HopSo, NULL, 2, s.MoTa, 1
    FROM @Seed s JOIN dbo.HangXe h ON h.TenHang=s.TenHang JOIN dbo.LoaiXe l ON l.TenLoai=s.TenLoai
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Xe x WITH (UPDLOCK,HOLDLOCK)
                      WHERE x.MaHang=h.MaHang AND x.TenXe=s.TenXe);

    INSERT dbo.MauXe (MaXe, TenMau, MaHex)
    SELECT i.MaXe,c.TenMau,NULL FROM @Inserted i JOIN @Colors c ON c.TenXe=i.TenXe;

    INSERT dbo.AnhXe (MaXe, MaMau, DuongDan, LaAnhChinh, ThuTu)
    SELECT i.MaXe,NULL,p.DuongDan,CASE WHEN p.ThuTu=0 THEN 1 ELSE 0 END,p.ThuTu
    FROM @Inserted i JOIN @Photos p ON p.TenXe=i.TenXe;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

SELECT DB_NAME() AS DatabaseDaNhap, COUNT(*) AS SoMauVuaThem FROM @Inserted;
SELECT MaXe,TenXe FROM @Inserted ORDER BY TenXe;
SELECT s.TenXe AS MauDaCoDuocBoQua FROM @Seed s
WHERE NOT EXISTS (SELECT 1 FROM @Inserted i WHERE i.TenXe=s.TenXe);
-- Chạy lại nguyên file: SoMauVuaThem = 0 nếu toàn bộ mẫu đã tồn tại.
