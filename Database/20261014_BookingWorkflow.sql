SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF COL_LENGTH('dbo.DatBan','MaCheckIn') IS NULL BEGIN
 ALTER TABLE dbo.DatBan ADD MaCheckIn varchar(32) NULL, TienCoc decimal(18,2) NOT NULL CONSTRAINT DF_DatBan_Coc DEFAULT 0,
 CocDaHoan bit NOT NULL CONSTRAINT DF_DatBan_HoanCoc DEFAULT 0, CheckInUtc datetime2 NULL, KetThucUtc datetime2 NULL,
 MaHoaDon varchar(40) NULL, DaThuTaiBan decimal(18,2) NOT NULL CONSTRAINT DF_DatBan_Thu DEFAULT 0, NhanVienThu int NULL;
END;
IF OBJECT_ID('dbo.GiuBan','U') IS NULL BEGIN
 CREATE TABLE dbo.GiuBan(Id uniqueidentifier PRIMARY KEY,MaKhachHang int NOT NULL REFERENCES dbo.KhachHang(MaKhachHang),MaBan int NOT NULL REFERENCES dbo.BanAn(MaBan),Ngay date NOT NULL,Gio time NOT NULL,SoKhach int NOT NULL,HetHanUtc datetime2 NOT NULL,MaDatBan int NULL REFERENCES dbo.DatBan(MaDatBan),GioHangJson nvarchar(max) NULL,GhiChu nvarchar(255) NULL);
 CREATE INDEX IX_GiuBan_Slot ON dbo.GiuBan(MaBan,Ngay,HetHanUtc);
 CREATE INDEX IX_GiuBan_Owner ON dbo.GiuBan(MaKhachHang,HetHanUtc);
END;
IF OBJECT_ID('dbo.ChiTietGoiMon','U') IS NULL BEGIN
 CREATE TABLE dbo.ChiTietGoiMon(Id int IDENTITY PRIMARY KEY,MaDatBan int NOT NULL REFERENCES dbo.DatBan(MaDatBan),MaMon int NOT NULL REFERENCES dbo.MonAn(MaMon),TenMon nvarchar(100) NOT NULL,SoLuong int NOT NULL CHECK(SoLuong BETWEEN 1 AND 99),GiaGoc decimal(18,2) NOT NULL,Giam decimal(5,2) NOT NULL,DonGia decimal(18,2) NOT NULL,DatTruoc bit NOT NULL,RequestId uniqueidentifier NOT NULL);
 CREATE UNIQUE INDEX UX_ChiTiet_Request ON dbo.ChiTietGoiMon(RequestId);
 CREATE INDEX IX_ChiTiet_DatBan ON dbo.ChiTietGoiMon(MaDatBan);
END;
COMMIT;
GO
UPDATE dbo.DatBan SET MaCheckIn='NH-'+CONVERT(varchar(20),CRYPT_GEN_RANDOM(10),2) WHERE MaCheckIn IS NULL;
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='UX_DatBan_CheckIn' AND object_id=OBJECT_ID('dbo.DatBan')) CREATE UNIQUE INDEX UX_DatBan_CheckIn ON dbo.DatBan(MaCheckIn) WHERE MaCheckIn IS NOT NULL;
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='UX_DatBan_Invoice' AND object_id=OBJECT_ID('dbo.DatBan')) CREATE UNIQUE INDEX UX_DatBan_Invoice ON dbo.DatBan(MaHoaDon) WHERE MaHoaDon IS NOT NULL;

