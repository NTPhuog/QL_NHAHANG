SET XACT_ABORT ON;
IF COL_LENGTH('dbo.BanAn','ViTri') IS NULL ALTER TABLE dbo.BanAn ADD ViTri nvarchar(150) NULL;
IF COL_LENGTH('dbo.MonAn','NgayThem') IS NULL ALTER TABLE dbo.MonAn ADD NgayThem datetime2 NULL;
GO
BEGIN TRANSACTION;
WHILE (SELECT COUNT(*) FROM dbo.BanAn WHERE SoChoNgoi<=2)<6
BEGIN
 DECLARE @n2 int=(SELECT ISNULL(MAX(MaBan),0)+1 FROM dbo.BanAn);
 INSERT dbo.BanAn(TenBan,SoChoNgoi,TrangThai) VALUES(CONCAT(N'Bàn ',FORMAT(@n2,'00')),2,N'Trống');
END;
;WITH positions AS(SELECT *,ROW_NUMBER() OVER(ORDER BY MaBan) rn FROM dbo.BanAn WHERE SoChoNgoi<=2)
UPDATE positions SET ViTri=COALESCE(ViTri,CONCAT(N'Tầng trệt · Khu A · Cửa sổ ',rn)),HinhAnh=CASE WHEN HinhAnh IS NULL OR HinhAnh='' THEN '/uploads/gallery/table-2.png' ELSE HinhAnh END;
WHILE (SELECT COUNT(*) FROM dbo.BanAn WHERE SoChoNgoi BETWEEN 3 AND 6)<5
BEGIN
 DECLARE @n6 int=(SELECT ISNULL(MAX(MaBan),0)+1 FROM dbo.BanAn);
 INSERT dbo.BanAn(TenBan,SoChoNgoi,TrangThai) VALUES(CONCAT(N'Bàn ',FORMAT(@n6,'00')),6,N'Trống');
END;
;WITH positions AS(SELECT *,ROW_NUMBER() OVER(ORDER BY MaBan) rn FROM dbo.BanAn WHERE SoChoNgoi BETWEEN 3 AND 6)
UPDATE positions SET ViTri=COALESCE(ViTri,CONCAT(N'Tầng trệt · Khu B · Gia đình ',rn)),HinhAnh=CASE WHEN HinhAnh IS NULL OR HinhAnh='' THEN '/uploads/gallery/table-6.png' ELSE HinhAnh END;
WHILE (SELECT COUNT(*) FROM dbo.BanAn WHERE SoChoNgoi BETWEEN 7 AND 12)<4
BEGIN
 DECLARE @n12 int=(SELECT ISNULL(MAX(MaBan),0)+1 FROM dbo.BanAn);
 INSERT dbo.BanAn(TenBan,SoChoNgoi,TrangThai) VALUES(CONCAT(N'Bàn ',FORMAT(@n12,'00')),12,N'Trống');
END;
;WITH positions AS(SELECT *,ROW_NUMBER() OVER(ORDER BY MaBan) rn FROM dbo.BanAn WHERE SoChoNgoi BETWEEN 7 AND 12)
UPDATE positions SET ViTri=COALESCE(ViTri,CONCAT(N'Tầng 1 · Khu C · Nhóm bạn ',rn)),HinhAnh=CASE WHEN HinhAnh IS NULL OR HinhAnh='' THEN '/uploads/gallery/table-12.png' ELSE HinhAnh END;
WHILE (SELECT COUNT(*) FROM dbo.BanAn WHERE SoChoNgoi BETWEEN 13 AND 24)<2
BEGIN
 DECLARE @n24 int=(SELECT ISNULL(MAX(MaBan),0)+1 FROM dbo.BanAn);
 INSERT dbo.BanAn(TenBan,SoChoNgoi,TrangThai) VALUES(CONCAT(N'Bàn ',FORMAT(@n24,'00')),24,N'Trống');
END;
;WITH positions AS(SELECT *,ROW_NUMBER() OVER(ORDER BY MaBan) rn FROM dbo.BanAn WHERE SoChoNgoi BETWEEN 13 AND 24)
UPDATE positions SET ViTri=COALESCE(ViTri,CONCAT(N'Tầng 1 · Khu D · Phòng riêng ',rn)),HinhAnh=CASE WHEN HinhAnh IS NULL OR HinhAnh='' THEN '/uploads/gallery/table-24.png' ELSE HinhAnh END;
IF NOT EXISTS(SELECT 1 FROM dbo.DanhMucMon WHERE TenDanhMuc=N'Khai vị') INSERT dbo.DanhMucMon(TenDanhMuc) VALUES(N'Khai vị');
IF NOT EXISTS(SELECT 1 FROM dbo.DanhMucMon WHERE TenDanhMuc=N'Món chính') INSERT dbo.DanhMucMon(TenDanhMuc) VALUES(N'Món chính');
IF NOT EXISTS(SELECT 1 FROM dbo.DanhMucMon WHERE TenDanhMuc=N'Tráng miệng') INSERT dbo.DanhMucMon(TenDanhMuc) VALUES(N'Tráng miệng');
IF NOT EXISTS(SELECT 1 FROM dbo.MonAn WHERE TenMon=N'Gỏi cuốn tôm') INSERT dbo.MonAn(MaDanhMuc,TenMon,MoTa,Gia,HinhAnh,TrangThai,NgayThem) SELECT TOP(1) MaDanhMuc,N'Gỏi cuốn tôm',N'Tôm, rau thơm và bún cuốn trong bánh tráng, dùng cùng sốt đậu phộng.',69000,'/uploads/gallery/goi-cuon.png',N'Hiển thị',DATEADD(hour,7,SYSUTCDATETIME()) FROM dbo.DanhMucMon WHERE TenDanhMuc=N'Khai vị' ORDER BY MaDanhMuc;
IF NOT EXISTS(SELECT 1 FROM dbo.MonAn WHERE TenMon=N'Cơm gà nướng sả') INSERT dbo.MonAn(MaDanhMuc,TenMon,MoTa,Gia,HinhAnh,TrangThai,NgayThem) SELECT TOP(1) MaDanhMuc,N'Cơm gà nướng sả',N'Gà nướng sả thơm lừng, ăn kèm cơm và rau củ.',129000,'/uploads/gallery/ga-nuong.png',N'Hiển thị',DATEADD(hour,7,SYSUTCDATETIME()) FROM dbo.DanhMucMon WHERE TenDanhMuc=N'Món chính' ORDER BY MaDanhMuc;
IF NOT EXISTS(SELECT 1 FROM dbo.MonAn WHERE TenMon=N'Cơm chiên hải sản') INSERT dbo.MonAn(MaDanhMuc,TenMon,MoTa,Gia,HinhAnh,TrangThai,NgayThem) SELECT TOP(1) MaDanhMuc,N'Cơm chiên hải sản',N'Cơm chiên tơi hạt với tôm, mực và rau củ.',119000,'/uploads/gallery/com-hai-san.png',N'Hiển thị',DATEADD(hour,7,SYSUTCDATETIME()) FROM dbo.DanhMucMon WHERE TenDanhMuc=N'Món chính' ORDER BY MaDanhMuc;
IF NOT EXISTS(SELECT 1 FROM dbo.MonAn WHERE TenMon=N'Cá hồi sốt bơ chanh') INSERT dbo.MonAn(MaDanhMuc,TenMon,MoTa,Gia,HinhAnh,TrangThai,NgayThem) SELECT TOP(1) MaDanhMuc,N'Cá hồi sốt bơ chanh',N'Cá hồi áp chảo, sốt bơ chanh và măng tây.',219000,'/uploads/gallery/ca-hoi.png',N'Hiển thị',DATEADD(hour,7,SYSUTCDATETIME()) FROM dbo.DanhMucMon WHERE TenDanhMuc=N'Món chính' ORDER BY MaDanhMuc;
IF NOT EXISTS(SELECT 1 FROM dbo.MonAn WHERE TenMon=N'Chả giò giòn') INSERT dbo.MonAn(MaDanhMuc,TenMon,MoTa,Gia,HinhAnh,TrangThai,NgayThem) SELECT TOP(1) MaDanhMuc,N'Chả giò giòn',N'Chả giò chiên giòn dùng cùng rau sống và nước chấm.',79000,'/uploads/gallery/cha-gio.png',N'Hiển thị',DATEADD(hour,7,SYSUTCDATETIME()) FROM dbo.DanhMucMon WHERE TenDanhMuc=N'Khai vị' ORDER BY MaDanhMuc;
IF NOT EXISTS(SELECT 1 FROM dbo.MonAn WHERE TenMon=N'Bánh flan caramel') INSERT dbo.MonAn(MaDanhMuc,TenMon,MoTa,Gia,HinhAnh,TrangThai,NgayThem) SELECT TOP(1) MaDanhMuc,N'Bánh flan caramel',N'Bánh flan mềm mịn, phủ caramel thơm dịu.',39000,'/uploads/gallery/banh-flan.png',N'Hiển thị',DATEADD(hour,7,SYSUTCDATETIME()) FROM dbo.DanhMucMon WHERE TenDanhMuc=N'Tráng miệng' ORDER BY MaDanhMuc;
COMMIT;
