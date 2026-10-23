-- =========================================================================================
-- ĐỒ ÁN CAPSTONE ECO2415 - ĐỀ TÀI 08: TOUR DU LỊCH HUẾ (HUE TOUR BOOKING)
-- CSDL SQL SERVER: BẢNG GỐC VÀ CỘT ĐẶT TÊN TIẾNG VIỆT, CÓ ĐẦY ĐỦ PK/FK VÀ DỮ LIỆU MẪU.
-- Script chỉ tạo mới HueTourDb khi CSDL chưa tồn tại; không tự xóa dữ liệu hiện có.
-- Các VIEW tên tiếng Anh giữ tương thích với ứng dụng hiện tại.
-- =========================================================================================

USE [master];
GO

IF DB_ID(N'HueTourDb') IS NOT NULL
    THROW 51004, N'HueTourDb đã tồn tại. Dừng để tránh xóa dữ liệu hiện có.', 1;
GO

CREATE DATABASE [HueTourDb];
GO
USE [HueTourDb];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

-- 1. TÀI KHOẢN NGƯỜI DÙNG VÀ PHÂN QUYỀN
CREATE TABLE dbo.TaiKhoanNguoiDung (
    MaTaiKhoan INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TaiKhoanNguoiDung PRIMARY KEY,
    TenDangNhap NVARCHAR(50) NOT NULL CONSTRAINT UQ_TaiKhoanNguoiDung_TenDangNhap UNIQUE,
    MatKhauBam NVARCHAR(255) NOT NULL,
    HoTen NVARCHAR(150) NOT NULL,
    VaiTro NVARCHAR(50) NOT NULL CONSTRAINT DF_TaiKhoanNguoiDung_VaiTro DEFAULT N'Customer',
    Email NVARCHAR(150) NULL,
    SoDienThoai NVARCHAR(20) NULL,
    NgayTao DATETIME2 NOT NULL CONSTRAINT DF_TaiKhoanNguoiDung_NgayTao DEFAULT SYSDATETIME(),
    CONSTRAINT CK_TaiKhoanNguoiDung_VaiTro CHECK (VaiTro IN (N'Admin', N'Customer'))
);
GO

-- 2. HỒ SƠ KHÁCH HÀNG
CREATE TABLE dbo.KhachHang (
    MaKhachHang INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_KhachHang PRIMARY KEY,
    MaTaiKhoan INT NULL,
    HoTen NVARCHAR(150) NOT NULL,
    Email NVARCHAR(150) NOT NULL,
    SoDienThoai NVARCHAR(20) NOT NULL,
    DiaChi NVARCHAR(255) NOT NULL,
    SoGiayToTuyThan NVARCHAR(50) NULL,
    NgayTao DATETIME2 NOT NULL CONSTRAINT DF_KhachHang_NgayTao DEFAULT SYSDATETIME(),
    CONSTRAINT FK_KhachHang_TaiKhoanNguoiDung FOREIGN KEY (MaTaiKhoan)
        REFERENCES dbo.TaiKhoanNguoiDung(MaTaiKhoan)
);
GO
-- Một tài khoản gắn tối đa một hồ sơ; khách vãng lai không cần tài khoản.
CREATE UNIQUE NONCLUSTERED INDEX UX_KhachHang_MaTaiKhoan
    ON dbo.KhachHang(MaTaiKhoan) WHERE MaTaiKhoan IS NOT NULL;
CREATE UNIQUE NONCLUSTERED INDEX UX_KhachHang_SoDienThoai ON dbo.KhachHang(SoDienThoai);
CREATE UNIQUE NONCLUSTERED INDEX UX_KhachHang_Email ON dbo.KhachHang(Email);
GO

-- 3. QUY TẮC GIÁ VÉ THEO LOẠI HÀNH KHÁCH
CREATE TABLE dbo.QuyTacGiaVe (
    MaQuyTac INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_QuyTacGiaVe PRIMARY KEY,
    LoaiHanhKhach NVARCHAR(50) NOT NULL CONSTRAINT UQ_QuyTacGiaVe_Loai UNIQUE,
    TenHienThi NVARCHAR(100) NOT NULL,
    HeSoGia DECIMAL(5,2) NOT NULL CONSTRAINT DF_QuyTacGiaVe_HeSo DEFAULT 1.00,
    TuoiToiThieu INT NOT NULL,
    TuoiToiDa INT NULL,
    MoTa NVARCHAR(255) NOT NULL,
    DangApDung BIT NOT NULL CONSTRAINT DF_QuyTacGiaVe_DangApDung DEFAULT 1,
    CONSTRAINT CK_QuyTacGiaVe_HeSo CHECK (HeSoGia BETWEEN 0 AND 1),
    CONSTRAINT CK_QuyTacGiaVe_DoTuoi CHECK (TuoiToiThieu >= 0 AND (TuoiToiDa IS NULL OR TuoiToiDa >= TuoiToiThieu))
);
GO

-- 4. DANH MỤC TOUR DU LỊCH
CREATE TABLE dbo.TourDuLich (
    MaTour INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TourDuLich PRIMARY KEY,
    MaTourHienThi NVARCHAR(50) NOT NULL CONSTRAINT UQ_TourDuLich_MaTourHienThi UNIQUE,
    TenTour NVARCHAR(255) NOT NULL,
    DanhMuc NVARCHAR(100) NOT NULL,
    MoTaNgan NVARCHAR(500) NOT NULL,
    MoTaChiTiet NVARCHAR(MAX) NOT NULL,
    LichTrinh NVARCHAR(MAX) NOT NULL,
    DiemNoiBat NVARCHAR(1000) NOT NULL,
    ThoiLuong NVARCHAR(50) NOT NULL CONSTRAINT DF_TourDuLich_ThoiLuong DEFAULT N'1 Ngày',
    DiemKhoiHanh NVARCHAR(200) NOT NULL CONSTRAINT DF_TourDuLich_DiemKhoiHanh DEFAULT N'Trung tâm TP Huế',
    GiaCoBan DECIMAL(18,2) NOT NULL,
    DuongDanAnh NVARCHAR(500) NOT NULL,
    DonViToChuc NVARCHAR(200) NULL,
    NguonGia NVARCHAR(1000) NULL,
    NgayKiemTraGia DATE NULL,
    DangMoBan BIT NOT NULL CONSTRAINT DF_TourDuLich_DangMoBan DEFAULT 1,
    NgayTao DATETIME2 NOT NULL CONSTRAINT DF_TourDuLich_NgayTao DEFAULT SYSDATETIME(),
    CONSTRAINT CK_TourDuLich_Gia CHECK (GiaCoBan >= 0)
);
GO

-- 5. LỊCH KHỞI HÀNH CỦA TOUR
CREATE TABLE dbo.ChuyenKhoiHanh (
    MaChuyen INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ChuyenKhoiHanh PRIMARY KEY,
    MaTour INT NOT NULL,
    NgayKhoiHanh DATETIME2 NOT NULL,
    NgayKetThuc DATETIME2 NOT NULL,
    SucChuaToiDa INT NOT NULL CONSTRAINT DF_ChuyenKhoiHanh_SucChua DEFAULT 25,
    SoChoDaDat INT NOT NULL CONSTRAINT DF_ChuyenKhoiHanh_SoCho DEFAULT 0,
    TrangThai NVARCHAR(50) NOT NULL CONSTRAINT DF_ChuyenKhoiHanh_TrangThai DEFAULT N'Open',
    DiemHen NVARCHAR(255) NOT NULL CONSTRAINT DF_ChuyenKhoiHanh_DiemHen DEFAULT N'Bến thuyền Tòa Khâm / Phu Văn Lâu',
    TenHuongDanVien NVARCHAR(100) NULL,
    SoDienThoaiHuongDanVien NVARCHAR(20) NULL,
    CONSTRAINT FK_ChuyenKhoiHanh_TourDuLich FOREIGN KEY (MaTour)
        REFERENCES dbo.TourDuLich(MaTour) ON DELETE CASCADE,
    CONSTRAINT CK_ChuyenKhoiHanh_ThoiGian CHECK (NgayKetThuc >= NgayKhoiHanh),
    CONSTRAINT CK_ChuyenKhoiHanh_SucChua CHECK (SucChuaToiDa > 0 AND SoChoDaDat BETWEEN 0 AND SucChuaToiDa),
    CONSTRAINT CK_ChuyenKhoiHanh_TrangThai CHECK (TrangThai IN (N'Open', N'Closed', N'Completed', N'Cancelled'))
);
GO
CREATE NONCLUSTERED INDEX IX_ChuyenKhoiHanh_Tour_Ngay ON dbo.ChuyenKhoiHanh(MaTour, NgayKhoiHanh);
GO

-- 6. ĐƠN ĐẶT TOUR (PHẦN ĐẦU ĐƠN)
CREATE TABLE dbo.DonDatTour (
    MaDonDatTour INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DonDatTour PRIMARY KEY,
    MaDatTour NVARCHAR(50) NOT NULL CONSTRAINT UQ_DonDatTour_MaDatTour UNIQUE,
    MaChuyen INT NOT NULL,
    MaKhachHang INT NOT NULL,
    NgayDat DATETIME2 NOT NULL CONSTRAINT DF_DonDatTour_NgayDat DEFAULT SYSDATETIME(),
    TongTien DECIMAL(18,2) NOT NULL,
    TrangThai NVARCHAR(50) NOT NULL CONSTRAINT DF_DonDatTour_TrangThai DEFAULT N'Pending',
    GhiChu NVARCHAR(500) NULL,
    YeuCauDacBiet NVARCHAR(500) NULL,
    PhuongThucThanhToan NVARCHAR(100) NOT NULL CONSTRAINT DF_DonDatTour_PhuongThuc DEFAULT N'Thanh toán trực tiếp / QR Code',
    CONSTRAINT FK_DonDatTour_ChuyenKhoiHanh FOREIGN KEY (MaChuyen)
        REFERENCES dbo.ChuyenKhoiHanh(MaChuyen),
    CONSTRAINT FK_DonDatTour_KhachHang FOREIGN KEY (MaKhachHang)
        REFERENCES dbo.KhachHang(MaKhachHang),
    CONSTRAINT CK_DonDatTour_TongTien CHECK (TongTien >= 0),
    CONSTRAINT CK_DonDatTour_TrangThai CHECK (TrangThai IN (N'Pending', N'Confirmed', N'Completed', N'Cancelled'))
);
GO
CREATE NONCLUSTERED INDEX IX_DonDatTour_MaChuyen ON dbo.DonDatTour(MaChuyen);
CREATE NONCLUSTERED INDEX IX_DonDatTour_MaKhachHang ON dbo.DonDatTour(MaKhachHang);
GO

-- 7. VÉ HÀNH KHÁCH (CHI TIẾT ĐƠN)
CREATE TABLE dbo.VeHanhKhach (
    MaVeHanhKhach INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_VeHanhKhach PRIMARY KEY,
    MaDonDatTour INT NOT NULL,
    MaVe NVARCHAR(50) NOT NULL CONSTRAINT UQ_VeHanhKhach_MaVe UNIQUE,
    HoTenHanhKhach NVARCHAR(150) NOT NULL,
    GioiTinh NVARCHAR(20) NOT NULL CONSTRAINT DF_VeHanhKhach_GioiTinh DEFAULT N'Nam',
    LoaiHanhKhach NVARCHAR(50) NOT NULL,
    NamSinh INT NOT NULL,
    GiaVe DECIMAL(18,2) NOT NULL,
    SoGhe NVARCHAR(20) NULL,
    GhiChu NVARCHAR(255) NULL,
    CONSTRAINT FK_VeHanhKhach_DonDatTour FOREIGN KEY (MaDonDatTour)
        REFERENCES dbo.DonDatTour(MaDonDatTour) ON DELETE CASCADE,
    CONSTRAINT FK_VeHanhKhach_QuyTacGiaVe FOREIGN KEY (LoaiHanhKhach)
        REFERENCES dbo.QuyTacGiaVe(LoaiHanhKhach),
    CONSTRAINT CK_VeHanhKhach_Gia CHECK (GiaVe >= 0)
);
GO
CREATE NONCLUSTERED INDEX IX_VeHanhKhach_MaDonDatTour ON dbo.VeHanhKhach(MaDonDatTour);
CREATE NONCLUSTERED INDEX IX_VeHanhKhach_LoaiHanhKhach ON dbo.VeHanhKhach(LoaiHanhKhach);
GO

-- 8. ĐỊA ĐIỂM ẨM THỰC (NHÀ HÀNG · QUÁN CÀ PHÊ · TIỆM BÁNH)
-- Nguồn dữ liệu: scripts/import_food_places.py đọc backend/data của dự án mẫu HUEGO.
-- Nạp dữ liệu: chạy database/seed/DiaDiemAmThuc_Seed.sql sau khi tạo bảng này.
CREATE TABLE dbo.DanhMucAmThuc (
    MaDanhMuc NVARCHAR(50) NOT NULL CONSTRAINT PK_DanhMucAmThuc PRIMARY KEY,
    TenDanhMuc NVARCHAR(80) NOT NULL CONSTRAINT UQ_DanhMucAmThuc_Ten UNIQUE
);
GO
INSERT INTO dbo.DanhMucAmThuc (MaDanhMuc, TenDanhMuc) VALUES
    (N'bakery', N'Tiệm bánh'),
    (N'bar', N'Quán bar'),
    (N'cafe', N'Quán cà phê'),
    (N'dessert', N'Tiệm bánh ngọt · kem'),
    (N'grill', N'Quán nướng'),
    (N'hotpot', N'Quán lẩu'),
    (N'noodle', N'Quán mì · bún'),
    (N'restaurant', N'Nhà hàng'),
    (N'seafood', N'Nhà hàng hải sản'),
    (N'vegetarian', N'Nhà hàng chay');
GO
CREATE TABLE dbo.DiaDiemAmThuc (
    MaDiaDiem INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DiaDiemAmThuc PRIMARY KEY,
    Slug NVARCHAR(160) NOT NULL CONSTRAINT UQ_DiaDiemAmThuc_Slug UNIQUE,
    TenDiaDiem NVARCHAR(250) NOT NULL,
    MaDanhMuc NVARCHAR(50) NOT NULL,
    MonDacTrung NVARCHAR(120) NULL,
    MoTa NVARCHAR(1000) NULL,
    DiaChi NVARCHAR(400) NULL,
    PhuongXa NVARCHAR(120) NULL,
    ToaDoLat DECIMAL(10,7) NULL,
    ToaDoLng DECIMAL(10,7) NULL,
    SoDienThoai NVARCHAR(40) NULL,
    GiaThamKhao DECIMAL(18,0) NULL,
    DiemDanhGia DECIMAL(3,2) NULL,
    SoDanhGia INT NULL,
    AnhMinhHoa NVARCHAR(400) NULL,
    AnhGoc NVARCHAR(600) NULL,
    Nguon NVARCHAR(80) NULL,
    DuocDoiChieu BIT NOT NULL CONSTRAINT DF_DiaDiemAmThuc_DuocDoiChieu DEFAULT 0,
    NgayTao DATETIME2(0) NOT NULL CONSTRAINT DF_DiaDiemAmThuc_NgayTao DEFAULT SYSDATETIME(),
    CONSTRAINT FK_DiaDiemAmThuc_DanhMucAmThuc FOREIGN KEY (MaDanhMuc)
        REFERENCES dbo.DanhMucAmThuc(MaDanhMuc),
    CONSTRAINT CK_DiaDiemAmThuc_DiemDanhGia CHECK (DiemDanhGia IS NULL OR (DiemDanhGia >= 0 AND DiemDanhGia <= 5)),
    CONSTRAINT CK_DiaDiemAmThuc_Gia CHECK (GiaThamKhao IS NULL OR GiaThamKhao >= 0)
);
GO
CREATE NONCLUSTERED INDEX IX_DiaDiemAmThuc_MaDanhMuc ON dbo.DiaDiemAmThuc(MaDanhMuc);
CREATE NONCLUSTERED INDEX IX_DiaDiemAmThuc_DiemDanhGia ON dbo.DiaDiemAmThuc(DiemDanhGia DESC);
GO

-- VIEW tương thích: ứng dụng Dapper đang dùng tên bảng/cột tiếng Anh.
-- Các VIEW chỉ chiếu trực tiếp một bảng nên giữ khả năng SELECT/INSERT/UPDATE/DELETE.
CREATE VIEW dbo.UserAccount AS
SELECT MaTaiKhoan AS Id, TenDangNhap AS Username, MatKhauBam AS PasswordHash,
       HoTen AS FullName, VaiTro AS Role, Email, SoDienThoai AS PhoneNumber, NgayTao AS CreatedAt
FROM dbo.TaiKhoanNguoiDung;
GO
CREATE VIEW dbo.Customer AS
SELECT MaKhachHang AS Id, MaTaiKhoan AS UserAccountId, HoTen AS FullName, Email,
       SoDienThoai AS PhoneNumber, DiaChi AS Address, SoGiayToTuyThan AS IdentityCard, NgayTao AS CreatedAt
FROM dbo.KhachHang;
GO
CREATE VIEW dbo.PriceRule AS
SELECT MaQuyTac AS Id, LoaiHanhKhach AS PassengerType, TenHienThi AS DisplayName,
       HeSoGia AS RateMultiplier, TuoiToiThieu AS MinAge, TuoiToiDa AS MaxAge,
       MoTa AS Description, DangApDung AS IsActive
FROM dbo.QuyTacGiaVe;
GO
CREATE VIEW dbo.Tour AS
SELECT MaTour AS Id, MaTourHienThi AS Code, TenTour AS Title, DanhMuc AS Category,
       MoTaNgan AS ShortDescription, MoTaChiTiet AS Description, LichTrinh AS Itinerary,
       DiemNoiBat AS Highlights, ThoiLuong AS Duration, DiemKhoiHanh AS StartingLocation,
       GiaCoBan AS BasePrice, DuongDanAnh AS ImageUrl, DonViToChuc AS ProviderName,
       NguonGia AS SourceUrl, NgayKiemTraGia AS PriceVerifiedAt,
       DangMoBan AS IsActive, NgayTao AS CreatedAt
FROM dbo.TourDuLich;
GO
CREATE VIEW dbo.Departure AS
SELECT MaChuyen AS Id, MaTour AS TourId, NgayKhoiHanh AS DepartureDate, NgayKetThuc AS ReturnDate,
       SucChuaToiDa AS MaxCapacity, SoChoDaDat AS BookedSeats, TrangThai AS Status,
       DiemHen AS MeetingPoint, TenHuongDanVien AS TourGuideName,
       SoDienThoaiHuongDanVien AS TourGuidePhone
FROM dbo.ChuyenKhoiHanh;
GO
CREATE VIEW dbo.TourBooking AS
SELECT MaDonDatTour AS Id, MaDatTour AS BookingCode, MaChuyen AS DepartureId,
       MaKhachHang AS CustomerId, NgayDat AS BookingDate, TongTien AS TotalAmount,
       TrangThai AS Status, GhiChu AS Notes, YeuCauDacBiet AS SpecialRequests,
       PhuongThucThanhToan AS PaymentMethod
FROM dbo.DonDatTour;
GO
CREATE VIEW dbo.PassengerTicket AS
SELECT MaVeHanhKhach AS Id, MaDonDatTour AS BookingId, MaVe AS TicketCode,
       HoTenHanhKhach AS FullName, GioiTinh AS Gender, LoaiHanhKhach AS PassengerType,
       NamSinh AS BirthYear, GiaVe AS Price, SoGhe AS SeatNumber, GhiChu AS Note
FROM dbo.VeHanhKhach;
GO
CREATE VIEW dbo.FoodPlace AS
SELECT MaDiaDiem AS Id, Slug, TenDiaDiem AS Name, DanhMuc AS Category, MaDanhMuc AS CategoryCode,
       MonDacTrung AS SignatureDish, MoTa AS Description, DiaChi AS Address, PhuongXa AS Ward,
       CAST(ToaDoLat AS FLOAT) AS Latitude, CAST(ToaDoLng AS FLOAT) AS Longitude, SoDienThoai AS Phone,
       GiaThamKhao AS AveragePrice, CAST(DiemDanhGia AS FLOAT) AS Rating, SoDanhGia AS ReviewCount,
       AnhMinhHoa AS ImageUrl, AnhGoc AS SourceImageUrl, Nguon AS Source,
       DuocDoiChieu AS IsCurated, NgayTao AS CreatedAt
FROM dbo.DiaDiemAmThuc;
GO
-- =========================================================================================
-- DỮ LIỆU MẪU BAN ĐẦU
-- =========================================================================================

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @Now DATETIME2(0) = SYSDATETIME();
    DECLARE @DepartureDaiNoi INT;
    DECLARE @DepartureTamGiang INT;
    DECLARE @DepartureSongHuong INT;
    DECLARE @DepartureBachMa INT;
    DECLARE @DepartureLichSu INT;

-- Nạp tài khoản mẫu
INSERT INTO dbo.UserAccount (Username, PasswordHash, FullName, Role, Email, PhoneNumber)
VALUES
('admin', 'pbkdf2-sha256$210000$TYpsCxfi81mkwnGOZQO5Dw==$ee1h28DQ2N+52LzN226i2AjDWQbHpxT7I/OYftdo7rU=', N'Quản Trị Viên Tour Huế', 'Admin', 'admin@huetour.vn', '0905123456'),
('khachhang', 'pbkdf2-sha256$210000$oZw35LYF0o9xo8lOC1bygQ==$NLXpsp0sk3NANY+daDzdrc6sVxJXu441JuHH/VsI5xQ=', N'Nguyễn Văn An', 'Customer', 'an.nguyen@gmail.com', '0912345678');

-- Nạp quy tắc giá mẫu (Luật 2: người lớn 100%, trẻ em 50%, em bé miễn phí)
INSERT INTO dbo.PriceRule (PassengerType, DisplayName, RateMultiplier, MinAge, MaxAge, Description, IsActive)
VALUES
('Adult', N'Người lớn', 1.00, 12, NULL, N'Hành khách từ 12 tuổi trở lên tính 100% giá gốc', 1),
('Child', N'Trẻ em (5 - 11 tuổi)', 0.50, 5, 11, N'Trẻ em từ 5 đến 11 tuổi tính 50% giá gốc', 1),
('Infant', N'Em bé (Dưới 5 tuổi)', 0.00, 0, 4, N'Em bé dưới 5 tuổi miễn phí vé tour (0 VNĐ)', 1);

-- Nạp danh sách tour mẫu
INSERT INTO dbo.Tour (Code, Title, Category, ShortDescription, Description, Itinerary, Highlights, Duration, StartingLocation, BasePrice, ImageUrl, IsActive)
VALUES
(
    'HT-DAINOI01',
    N'Tour Khám Phá Di Sản Hoàng Cung Đại Nội & Các Lăng Tẩm Triều Nguyễn',
    N'Di tích Cố Đô',
    N'Hành trình xuyên qua 143 năm vàng son của triều Nguyễn, chiêm ngưỡng Ngọ Môn, Điện Thái Hòa, Lăng Khải Định tráng lệ và Lăng Tự Đức thơ mộng.',
    N'Trải nghiệm văn hóa di sản hoàng gia trọn vẹn trong ngày. Bạn sẽ được hướng dẫn viên thuyết minh chi tiết về điển tích, nghệ thuật kiến trúc cung đình đỉnh cao, thưởng ngoạn cảnh quan sơn thủy hữu tình.',
    N'08:00 Đón khách tại trung tâm TP Huế -> 08:30 Tham quan Hoàng Thành Đại Nội (Ngọ Môn, Điện Thái Hòa, Tử Cấm Thành) -> 11:30 Thưởng thức bữa trưa cơm niêu Cố Đô -> 13:30 Viếng Lăng Khải Định lộng lẫy -> 15:30 Chiêm bái Lăng Tự Đức thi vị -> 17:00 Trả khách tại điểm hẹn.',
    N'Vé vào cổng trọn gói các điểm di tích; Xe du lịch đời mới máy lạnh; Thuyết minh viên chuyên nghiệp am hiểu lịch sử; Ăn trưa đặc sản Huế.',
    N'1 Ngày',
    N'Cột cờ Phu Văn Lâu, TP Huế',
    550000,
    '/images/imperial-city-2025.jpg',
    1
),
(
    'HT-TAMGIANG02',
    N'Tour Hoàng Hôn Đầm Phá Tam Giang & Trải Nghiệm Chèo Sup Đầm Chuồn',
    N'Sinh thái & Đầm phá',
    N'Ngắm nhìn phá Tam Giang - vùng đầm phá nước lợ lớn nhất Đông Nam Á lung linh trong ánh hoàng hôn, thưởng thức hải sản đầm Chuồn tươi sống.',
    N'Rời xa khói bụi thành phố, cùng ngư dân lênh đênh trên mặt nước mênh mông, đổ nò bắt tôm cá, chèo Sup thư giãn và thưởng thức tiệc hải sản tươi rói ngay tại nhà chồ giữa đầm.',
    N'14:00 Xe đón khách tại trung tâm Huế -> 14:45 Đến bến đầm Chuồn, lên thuyền tham quan -> 15:30 Trải nghiệm đổ nò, quăng chài bắt cá tôm cùng ngư dân -> 16:30 Chèo Sup, chụp ảnh hoàng hôn rực rỡ -> 18:00 Tiệc hải sản tươi sống tại nhà hàng nổi -> 19:30 Trở về TP Huế.',
    N'Thuyền du lịch đầm phá an toàn; Chèo Sup và áo phao miễn phí; Bữa tối hải sản đầm phá cực ngon (tôm, ghẹ, bánh khoái cá kình); Chụp ảnh hoàng hôn.',
    N'Nửa Ngày (Chiều tối)',
    N'Bến thuyền Đầm Chuồn, Phú Vang',
    480000,
    '/images/tam-giang-2024.jpg',
    1
),
(
    'HT-SONGHUONG03',
    N'Du Thuyền Sông Hương Ngắm Cố Đô & Nghe Ca Huế Thính Phòng Về Đêm',
    N'Sông Hương & Làng nghề',
    N'Thả hồn theo dòng Hương giang thơ mộng trên thuyền rồng đôi, lắng nghe điệu Nam ai, Nam bình da diết và thả hoa đăng cầu may mắn.',
    N'Ca Huế là Di sản văn hóa phi vật thể quốc gia. Trong không gian tĩnh lặng lung linh ánh đèn của cầu Tràng Tiền, những giai điệu nhã nhạc thính phòng đưa bạn về với hoài niệm Cố Đô sâu lắng.',
    N'19:00 Đón khách tại bến Tòa Khâm -> 19:30 Thuyền rồng rời bến, xuôi dòng Hương qua cầu Tràng Tiền, cầu Phú Xuân -> 20:00 Thưởng thức đêm biểu diễn Ca Huế của các nghệ sĩ lão thành -> 20:45 Trải nghiệm thả hoa đăng giấy màu -> 21:15 Cập bến Tòa Khâm.',
    N'Vé thuyền rồng đôi ngắm sông Hương; Đoàn nghệ nhân biểu diễn Ca Huế mộc; Mỗi khách được tặng hoa đăng thả sông; Trà cung đình Huế.',
    N'3 Giờ',
    N'Bến thuyền Tòa Khâm, 49 Lê Lợi, TP Huế',
    220000,
    'https://upload.wikimedia.org/wikipedia/commons/thumb/e/e1/Dragon_boat_on_the_Perfume_River_%2825606150197%29.jpg/1280px-Dragon_boat_on_the_Perfume_River_%2825606150197%29.jpg',
    1
),
(
    'HT-THUYBIEU04',
    N'Tour Sinh Thái Làng Cổ Thủy Biều & Ngâm Chân Thảo Dược Thanh Trà',
    N'Sông Hương & Làng nghề',
    N'Đạp xe dưới bóng râm mát rượi của những vườn thanh trà trĩu quả, tham quan nhà rường cổ hàng trăm năm tuổi và ngâm chân thảo mộc thảo dược làng quê.',
    N'Hành trình chữa lành và thư giãn tuyệt đối bên dòng sông Hương. Khám phá nét đẹp mộc mạc của làng cổ Thủy Biều, học làm mè xửng, làm bánh gói truyền thống Huế.',
    N'08:30 Đón khách tại chùa Thiên Mụ -> 09:00 Đi thuyền ngược dòng Hương lên Thủy Biều -> 09:30 Đạp xe quanh đường làng rợp bóng cây -> 10:30 Trải nghiệm làm bánh lọc, gói bánh nậm -> 11:30 Thư giãn ngâm chân thảo dược lá bưởi thanh trà -> 12:30 Ăn trưa ẩm thực nhà vườn.',
    N'Thuyền đưa đón dọc sông Hương; Xe đạp miễn phí suốt hành trình; Ngâm chân nước ấm thảo dược vườn nhà; Thưởng thức thanh trà và bữa trưa quê.',
    N'Nửa Ngày',
    N'Chùa Thiên Mụ / Làng Thủy Biều',
    390000,
    '/images/thien-mu-2025.jpg',
    1
),
(
    'HT-AMTHUCDEM05',
    N'Food Tour Ẩm Thực Cố Đô Đêm Bằng Xe Xích Lô Truyền Thống',
    N'Văn hóa & Ẩm thực',
    N'Ngồi xích lô dạo qua 36 phố phường đêm Huế, nếm thử 10 món ăn đặc sản trứ danh từ bánh bèo, nậm, lọc đến bún bò Huế và chè hẻm 20 món.',
    N'Huế là cái nôi ẩm thực tinh hoa của Việt Nam. Chuyến đi đưa bạn đến đúng những quán ăn gia truyền có tuổi đời hàng chục năm, nếm vị đậm đà cay nồng đúng chất Huế.',
    N'17:30 Bác xích lô đón khách tại khách sạn -> 18:00 Thưởng thức Bánh bèo - nậm - lọc bà Đỏ -> 19:00 Ăn Bún bò mụ Rơi nổi tiếng -> 20:00 Bánh ép Thuận An giòn rụm -> 20:45 Giải nhiệt với chè hẻm Cung Đình 20 món -> 21:30 Dạo mát quanh sông Hương và về lại điểm hẹn.',
    N'Xe xích lô và tài xế suốt buổi tối; Trọn gói chi phí thưởng thức 10 món ăn; Hướng dẫn viên bản địa sành ăn dẫn đường.',
    N'4 Giờ',
    N'Đón tại các khách sạn trung tâm TP Huế',
    420000,
    '/images/bun-bo-hue-2024.jpg',
    1
),
(
    'HT-BACHMA06',
    N'Tour Trekking Vườn Quốc Gia Bạch Mã & Chinh Phục Ngũ Hồ - Hải Vọng Đài',
    N'Sinh thái & Đầm phá',
    N'Khám phá thiên nhiên kỳ vĩ tại Bạch Mã, tắm suối mát lạnh ở cụm Ngũ Hồ và phóng tầm mắt chiêm ngưỡng toàn cảnh phá Cầu Hai từ đỉnh Hải Vọng Đài.',
    N'Dành cho những tâm hồn yêu thiên nhiên hoang sơ và vận động. Khí hậu mát lạnh quanh năm tựa Đà Lạt giữa lòng Cố Đô, ngắm thác Đỗ Quyên hùng vĩ cao hơn 300 mét.',
    N'07:30 Xe đón tại trung tâm Huế -> 09:00 Đến chân núi Bạch Mã -> 10:00 Chinh phục Hải Vọng Đài độ cao 1.450m -> 11:30 Trekking đường mòn Ngũ Hồ, ăn trưa picnic bên suối -> 13:30 Chiêm ngưỡng thác Đỗ Quyên tráng lệ -> 15:30 Xuống núi -> 17:30 Về đến TP Huế.',
    N'Xe trung chuyển hai chiều và xe chuyên dụng lên đỉnh Bạch Mã; Vé tham quan vườn quốc gia; Bữa trưa picnic thơm ngon; Hướng dẫn viên trekking có kinh nghiệm.',
    N'1 Ngày',
    N'Trung tâm TP Huế',
    680000,
    '/images/bach-ma-waterfall.jpg',
    1
),
(
    'HT-TUDUC07',
    N'Tour Lăng Tự Đức – Làng Hương Thủy Xuân – Đồi Vọng Cảnh',
    N'Di tích Cố Đô',
    N'Khám phá Lăng Tự Đức, sắc màu làng hương Thủy Xuân và ngắm sông Hương từ đồi Vọng Cảnh.',
    N'Hành trình kết hợp di sản và làng nghề truyền thống. Du khách tham quan lăng Tự Đức, tìm hiểu nghề làm hương tại Thủy Xuân và dừng chân ngắm cảnh sông Hương từ đồi Vọng Cảnh.',
    N'08:00 Đón khách tại trung tâm Huế -> 08:30 Tham quan Lăng Tự Đức -> 10:30 Ghé làng hương Thủy Xuân, trò chuyện cùng nghệ nhân -> 11:30 Ngắm cảnh tại đồi Vọng Cảnh -> 12:30 Trả khách tại trung tâm.',
    N'Di tích Lăng Tự Đức; Trải nghiệm làng nghề làm hương; Ngắm sông Hương từ đồi Vọng Cảnh; Hướng dẫn viên địa phương.',
    N'Nửa Ngày', N'Trung tâm TP Huế', 520000, '/images/tu-duc-tomb.jpg', 1
),
(
    'HT-THANHTOAN08',
    N'Tour Cầu Ngói Thanh Toàn – Chợ Quê – Làng Hoa Giấy Thanh Tiên',
    N'Sông Hương & Làng nghề',
    N'Thăm cây cầu ngói cổ, khám phá nhịp sống chợ quê và tìm hiểu nghề làm hoa giấy truyền thống xứ Huế.',
    N'Một buổi khám phá vùng quê Huế với kiến trúc cầu ngói Thanh Toàn, không gian chợ quê và hoạt động tìm hiểu nghề làm hoa giấy tại làng Thanh Tiên.',
    N'08:00 Đón khách tại trung tâm Huế -> 08:45 Tham quan Cầu Ngói Thanh Toàn và nhà trưng bày nông cụ -> 10:00 Dạo chợ quê -> 10:45 Đến làng Thanh Tiên, tìm hiểu cách làm hoa giấy -> 12:00 Thưởng thức bữa trưa địa phương -> 13:00 Trở về trung tâm.',
    N'Cầu ngói Thanh Toàn; Không gian chợ quê; Gặp gỡ nghệ nhân làng hoa giấy Thanh Tiên; Thưởng thức món ăn địa phương.',
    N'Nửa Ngày', N'Trung tâm TP Huế', 450000, '/images/thanh-toan-2018.jpg', 1
),
(
    'HT-LANGCO09',
    N'Tour Biển Lăng Cô – Đầm Lập An – Hải Vân Quan',
    N'Sinh thái & Đầm phá',
    N'Kết hợp biển xanh Lăng Cô, cảnh đầm Lập An dưới chân núi và điểm dừng ngắm cảnh Hải Vân Quan.',
    N'Khám phá vùng ven biển phía nam Huế với các điểm dừng nổi bật: ngắm cảnh đầm Lập An, thư giãn tại vịnh Lăng Cô và tham quan Hải Vân Quan. Lịch trình có thời gian di chuyển đường dài, phù hợp cho chuyến đi trong ngày.',
    N'07:00 Đón khách tại trung tâm Huế -> 08:30 Dừng ngắm cảnh tại Hải Vân Quan -> 10:00 Tham quan đầm Lập An -> 11:30 Ăn trưa hải sản tại Lăng Cô -> 13:00 Nghỉ ngơi và dạo biển -> 15:00 Khởi hành về Huế -> 17:00 Trả khách.',
    N'Vịnh biển Lăng Cô; Cảnh quan đầm Lập An; Hải Vân Quan; Xe đưa đón trong ngày và hướng dẫn viên.',
    N'1 Ngày', N'Trung tâm TP Huế', 850000, '/images/lang-co-2023.jpg', 1
),
(
    'HT-MINHMANG10',
    N'Tour Di Tích Lăng Minh Mạng – Lăng Gia Long – Đồi Vọng Cảnh',
    N'Di tích Cố Đô',
    N'Khám phá lăng tẩm uy nghiêm nhất triều Nguyễn, chiêm ngưỡng kiến trúc phong thủy đăng đối đỉnh cao bên dòng Hương giang.',
    N'Hành trình đưa quý khách đến với Lăng Gia Long giữa rừng thông bạt ngàn và Lăng Minh Mạng thanh tịnh, tôn nghiêm.',
    N'08:00 Đón khách tại trung tâm TP Huế -> 08:45 Khám phá Lăng Gia Long -> 10:30 Tham quan Lăng Minh Mạng uy nghiêm -> 11:45 Ngắm sông Hương từ Đồi Vọng Cảnh -> 12:30 Thưởng thức bữa trưa đặc sản Huế.',
    N'Xe du lịch đời mới; Vé tham quan các lăng; Hướng dẫn viên am tường lịch sử; Nước suối và khăn lạnh.',
    N'Nửa Ngày', N'Trung tâm TP Huế', 520000, '/images/minh-mang-2024.jpg', 1
),
(
    'HT-NONLA11',
    N'Tour Trải Nghiệm Làng Nghề Nón Lá Tây Hồ & Hoa Giấy Thanh Tiên',
    N'Sông Hương & Làng nghề',
    N'Tự tay chằm chiếc nón bài thơ xứ Huế và học gấp hoa sen giấy Thanh Tiên cùng các nghệ nhân lão thành.',
    N'Tour văn hóa làng nghề truyền thống đặc sắc. Du khách được tự tay làm nón bài thơ và hoa giấy truyền thống mang về làm kỷ niệm.',
    N'08:30 Đón khách -> 09:00 Đến làng nón Tây Hồ, trải nghiệm chằm nón bài thơ -> 10:30 Thăm làng hoa giấy Thanh Tiên -> 11:30 Thưởng trà sen và bánh đậu xanh trái cây -> 12:30 Trả khách.',
    N'Tự tay làm nón bài thơ mang về; Trò chuyện với nghệ nhân; Xe đưa đón; Trà sen cung đình.',
    N'Nửa Ngày', N'Trung tâm TP Huế', 420000, '/images/hue-conical-hat.jpg', 1
),
(
    'HT-SUOIVOI12',
    N'Tour Sinh Thái Suối Voi – Đầm Lập An – Vịnh Lăng Cô',
    N'Sinh thái & Đầm phá',
    N'Tuyến Suối Voi – Lập An – Lăng Cô đang tạm ẩn trong khi xác minh tình trạng khai thác tại điểm đến.',
    N'Tuyến gợi ý phía Nam Huế được giữ trong dữ liệu capstone để quản trị tham khảo. Suối Voi đang có dự án du lịch dang dở và thông tin về hoạt động tại điểm cần được xác nhận trước khi mở bán lại.',
    N'Tạm ngưng nhận đặt chỗ. Chỉ kích hoạt lại sau khi xác minh được hoạt động, dịch vụ và điều kiện tham quan tại Suối Voi.',
    N'Đầm Lập An; Vịnh Lăng Cô; Tình trạng Suối Voi cần xác minh trước khi tổ chức.',
    N'1 Ngày', N'Trung tâm TP Huế', 720000, '/images/lang-co-2023.jpg', 0
),
(
    'HT-DAINOI-THIENMU13', N'Đại Nội – Chùa Thiên Mụ – Nhà vườn An Hiên', N'Di tích Cố Đô',
    N'Khám phá Hoàng thành, ghé ngôi chùa bên sông Hương và tìm hiểu nếp nhà vườn Huế.',
    N'Lịch trình gợi ý kết hợp di sản trung tâm, cảnh quan sông Hương và kiến trúc nhà vườn. Thứ tự điểm đến có thể điều chỉnh theo giờ mở cửa và điều kiện thực tế.',
    N'08:00 Hẹn tại trung tâm Huế -> 08:30 Tham quan Đại Nội -> 11:30 Nghỉ trưa -> 13:30 Tham quan chùa Thiên Mụ -> 15:00 Ghé nhà vườn An Hiên -> 16:30 Kết thúc tại trung tâm.',
    N'Đại Nội Huế; chùa Thiên Mụ; nhà vườn An Hiên; tuyến di sản trong thành phố.',
    N'1 Ngày', N'Trung tâm TP Huế', 590000, '/images/thien-mu-2025.jpg', 1
),
(
    'HT-RUCHATAMGIANG14', N'Bình Minh Rú Chá – Phá Tam Giang – Làng chài', N'Sinh thái & Đầm phá',
    N'Ngắm bình minh giữa rừng ngập mặn Rú Chá và tìm hiểu nhịp sống ven phá Tam Giang.',
    N'Chuyến đi gợi ý dành cho người thích thiên nhiên và đời sống địa phương. Thời điểm ngắm bình minh, hoạt động trên thuyền và điểm dừng cần xác nhận trước với đơn vị vận hành.',
    N'05:00 Đón tại trung tâm -> 05:45 Dạo rừng ngập mặn Rú Chá -> 07:00 Ăn sáng địa phương -> 08:00 Tham quan vùng phá Tam Giang và làng chài -> 10:30 Trở về Huế.',
    N'Rừng ngập mặn Rú Chá; cảnh quan đầm phá; tìm hiểu sinh kế địa phương; khung giờ bình minh.',
    N'Nửa Ngày', N'Trung tâm TP Huế', 460000, 'https://hue.gov.vn/Portals/0/Uploads/00.00.H57/Nam2025/Thang6/z5862841503198458c3a50b5d9fa36ebde75e83e601fed_17271732773451054397863_28_4_2025_17_12_01_832_CH.jpg?v=1773705600033', 1
),
(
    'HT-SONGHUONGAMTHUC15', N'Dạo phố Huế – Chợ Đông Ba – Ẩm thực địa phương', N'Văn hóa & Ẩm thực',
    N'Đi bộ qua những điểm phố trung tâm, ghé chợ Đông Ba và thưởng thức các món Huế theo mùa.',
    N'Tuyến tham quan gợi ý kết hợp cảnh quan đô thị và ẩm thực. Món ăn, quán dừng và thời lượng có thể thay đổi theo ngày; giá đang hiển thị là dữ liệu mẫu.',
    N'16:00 Hẹn gần cầu Trường Tiền -> 16:30 Dạo bờ sông Hương -> 17:00 Tham quan khu vực chợ Đông Ba -> 18:00 Thưởng thức các món địa phương -> 19:30 Kết thúc tại trung tâm.',
    N'Cầu Trường Tiền; chợ Đông Ba; món ăn địa phương; trải nghiệm đi bộ trung tâm.',
    N'4 Giờ', N'Cầu Trường Tiền, TP Huế', 350000, '/images/dong-ba-market-2024.jpg', 1
),
(
    'HT-LANGMINHMANG16', N'Lăng Gia Long – Lăng Minh Mạng – Lăng Khải Định', N'Di tích Cố Đô',
    N'So sánh dấu ấn kiến trúc và cảnh quan của ba lăng vua Nguyễn trong chuyến đi một ngày.',
    N'Tuyến di sản gợi ý nối các lăng tẩm ở phía nam thành phố. Thời gian di chuyển giữa các điểm khá dài, cần kiểm tra giờ mở cửa, vé và phương tiện trước ngày đi.',
    N'07:30 Hẹn tại trung tâm -> 08:30 Tham quan Lăng Gia Long -> 10:30 Tham quan Lăng Minh Mạng -> 12:00 Nghỉ trưa -> 14:00 Tham quan Lăng Khải Định -> 16:30 Trở về trung tâm.',
    N'Lăng Gia Long; lăng Minh Mạng; lăng Khải Định; kiến trúc và cảnh quan lăng tẩm triều Nguyễn.',
    N'1 Ngày', N'Trung tâm TP Huế', 690000, '/images/minh-mang-2024.jpg', 1
),
(
    'HT-THUANAN-TAMGIANG17', N'Biển Thuận An – Phá Tam Giang – Đầm Chuồn', N'Sinh thái & Đầm phá',
    N'Kết hợp dạo biển Thuận An với cảnh quan đầm phá và trải nghiệm ẩm thực vùng ven Huế.',
    N'Lịch trình gợi ý theo mùa, kết hợp vùng biển và đầm phá phía đông Huế. Điều kiện thời tiết, tắm biển và hoạt động thuyền cần được xác nhận trước chuyến đi.',
    N'08:00 Hẹn tại trung tâm -> 08:45 Dạo biển Thuận An -> 11:00 Di chuyển đến vùng phá Tam Giang -> 12:00 Ăn trưa địa phương -> 14:00 Tham quan Đầm Chuồn -> 16:00 Trở về Huế.',
    N'Biển Thuận An; cảnh quan phá Tam Giang; Đầm Chuồn; ẩm thực địa phương.',
    N'1 Ngày', N'Trung tâm TP Huế', 560000, '/images/thuan-an-beach-2023.jpg', 1
),
(
    'HT-HOQUYEN18', N'Hổ Quyền – Điện Voi Ré – Lăng Tự Đức', N'Di tích Cố Đô',
    N'Tìm hiểu dấu tích trường đấu voi độc đáo, Điện Voi Ré và không gian lăng Tự Đức.',
    N'Tuyến di sản gợi ý ở phía tây nam Huế, nối Hổ Quyền với Điện Voi Ré và lăng Tự Đức. Cần kiểm tra giờ tham quan, vé và phương tiện trước khi khởi hành.',
    N'08:00 Hẹn tại trung tâm Huế -> 08:30 Tham quan Hổ Quyền -> 09:45 Tìm hiểu Điện Voi Ré -> 11:00 Tham quan lăng Tự Đức -> 12:30 Kết thúc tại trung tâm.',
    N'Di tích Hổ Quyền; Điện Voi Ré; lăng Tự Đức; kiến trúc và câu chuyện triều Nguyễn.',
    N'Nửa Ngày', N'Trung tâm TP Huế', 420000, '/images/ho-quyen-2008.jpg', 1
),
(
    'HT-HONCHEN19', N'Điện Hòn Chén – Đồi Vọng Cảnh – Sông Hương', N'Sông Hương & Làng nghề',
    N'Ghé không gian tín ngưỡng Điện Hòn Chén và ngắm khúc sông Hương từ đồi Vọng Cảnh.',
    N'Hành trình gợi ý kết hợp điểm di tích bên sông và cảnh quan đồi Vọng Cảnh. Nếu đi thuyền, cần xác nhận bến, giờ chạy, điều kiện thời tiết và chương trình lễ hội với đơn vị tổ chức.',
    N'07:30 Hẹn tại trung tâm Huế -> 08:15 Tham quan Điện Hòn Chén -> 10:30 Dừng ngắm cảnh tại đồi Vọng Cảnh -> 11:30 Trở về trung tâm. Phương án đường thủy chỉ thực hiện khi được xác nhận.',
    N'Điện Hòn Chén; đồi Vọng Cảnh; cảnh quan sông Hương; trải nghiệm văn hóa tín ngưỡng địa phương.',
    N'Nửa Ngày', N'Trung tâm TP Huế', 390000, '/images/hon-chen-2008.jpg', 1
),
(
    'HT-THANHTAN20', N'Suối khoáng nóng Thanh Tân – Nghỉ dưỡng giữa thiên nhiên', N'Thiên nhiên & Nghỉ dưỡng',
    N'Dành một ngày thư giãn với trải nghiệm suối khoáng nóng ở phía tây bắc Huế.',
    N'Tuyến nghỉ dưỡng tham khảo đến khu suối khoáng nóng Thanh Tân, cách trung tâm Huế khoảng 30 km. Hạng mục dịch vụ, giờ hoạt động, vé và phương tiện cần xác nhận trực tiếp với khu du lịch trước chuyến đi.',
    N'08:00 Khởi hành từ trung tâm Huế -> 09:00 Đến khu vực Thanh Tân -> 09:30–14:00 Tham quan và sử dụng dịch vụ đang mở -> 15:00 Trở về Huế. Hoạt động cụ thể phụ thuộc dịch vụ được xác nhận.',
    N'Khu suối khoáng nóng Thanh Tân; cảnh quan thiên nhiên; lịch trình nghỉ dưỡng trong ngày; dịch vụ phụ thuộc tình trạng vận hành.',
    N'1 Ngày', N'Trung tâm TP Huế', 750000, 'https://huetourism.gov.vn/Medias/Nam2023/Thang8/suoi-khoang-nong-thanh-tan.jpg', 1
),
(
    'HT-VANCU21', N'Làng bún Vân Cù – Phố cổ Bao Vinh – Làng Kim Long', N'Văn hóa & Ẩm thực',
    N'Tìm hiểu nghề làm bún truyền thống, dạo phố Bao Vinh và khám phá nếp sống ven sông.',
    N'Tuyến trải nghiệm cộng đồng gợi ý kết nối làng nghề Vân Cù, khu phố cổ Bao Vinh và làng Kim Long. Hoạt động gặp nghệ nhân, làm thử và nếm món cần đặt trước, tùy lịch của cộng đồng địa phương.',
    N'08:00 Hẹn tại trung tâm Huế -> 08:45 Tham quan làng nghề bún Vân Cù -> 10:30 Dạo khu phố Bao Vinh -> 11:30 Ghé làng Kim Long, tìm hiểu nghề và ẩm thực địa phương -> 13:00 Trở về trung tâm.',
    N'Nghề làm bún Vân Cù; không gian phố Bao Vinh; làng Kim Long; trải nghiệm ẩm thực và đời sống địa phương.',
    N'Nửa Ngày', N'Trung tâm TP Huế', 430000, 'https://upload.wikimedia.org/wikipedia/commons/thumb/b/bd/Bao_Vinh_street.jpg/1280px-Bao_Vinh_street.jpg', 1
),
(
    'HT-HONCHAO22', N'Đảo Hòn Chảo – Vịnh Lăng Cô', N'Sinh thái & Đầm phá',
    N'Ngắm cảnh biển Lăng Cô và trải nghiệm vùng đảo Hòn Chảo theo điều kiện thời tiết.',
    N'Tuyến biển tham khảo kết hợp vịnh Lăng Cô với hoạt động tham quan Hòn Chảo. Tàu thuyền, lặn ngắm san hô và các dịch vụ phải được xác nhận với đơn vị có tổ chức; lịch trình phụ thuộc thời tiết và an toàn hàng hải.',
    N'07:00 Khởi hành từ trung tâm Huế -> 08:30 Đến vùng Lăng Cô -> 09:00 Tham quan Hòn Chảo nếu tàu được phép hoạt động và thời tiết phù hợp -> 12:00 Nghỉ trưa -> 14:00 Trở về Huế. Nếu điều kiện không đảm bảo, đổi lịch hoặc dừng hoạt động.',
    N'Vịnh Lăng Cô; cảnh quan đảo Hòn Chảo; hoạt động biển theo điều kiện thực tế; ưu tiên an toàn thời tiết.',
    N'1 Ngày', N'Trung tâm TP Huế', 980000, '/images/lang-co-2023.jpg', 1
),
(
    'HT-DISANDEM23', N'Đại Nội Huế về đêm – Trải nghiệm di sản theo chương trình', N'Di tích Cố Đô',
    N'Gợi ý khám phá không gian Hoàng cung vào buổi tối khi có chương trình được công bố.',
    N'Một số chương trình đêm và dạ yến cung đình được tổ chức theo sự kiện hoặc lịch riêng. Tuyến này chỉ là gợi ý trong capstone; không mặc định hoạt động hằng ngày và cần xác nhận chương trình, vé, thời gian với đơn vị tổ chức.',
    N'Lịch trình thay đổi theo chương trình được công bố. Chỉ đặt chỗ sau khi xác nhận ngày tổ chức, vé vào cửa và nội dung dịch vụ.',
    N'Không gian Hoàng cung; hoạt động văn hóa buổi tối theo lịch; cần xác nhận chương trình trước khi đặt.',
    N'Theo chương trình', N'Đại Nội Huế', 650000, '/images/hue-imperial-night-2024.jpg', 1
);

-- Tour đang được niêm yết công khai trên HueTripO (kiểm tra giá 22/10/2026).
-- Giá là mức khởi điểm từ nhà cung cấp; lịch và chỗ trống cần xác nhận trực tiếp.
INSERT INTO dbo.Tour (Code, Title, Category, ShortDescription, Description, Itinerary, Highlights, Duration, StartingLocation, BasePrice, ImageUrl, ProviderName, SourceUrl, PriceVerifiedAt, IsActive)
VALUES
('HT-MARKET-THANHTAN24', N'[Huế] Huế – Suối khoáng nóng Thanh Tân – Huế', N'Thiên nhiên & Nghỉ dưỡng', N'Tour ghép một ngày từ Huế đến khu suối khoáng nóng Thanh Tân.', N'Tour được niêm yết trên HueTripO ngày 22/10/2026 với giá từ 950.000đ/khách. Lịch trình chi tiết, giá cuối cùng và dịch vụ cần xác nhận với Công ty CP Du lịch Đại Bàng.', N'Huế -> Khu suối khoáng nóng Thanh Tân -> Huế. Hoạt động theo xác nhận của đơn vị tổ chức.', N'Suối khoáng nóng Thanh Tân; tour ghép; giá tham khảo từ 950.000đ/khách.', N'1 Ngày', N'Huế', 950000, N'/images/thien-mu-2025.jpg', N'Công ty CP Du lịch Đại Bàng', N'https://huetripo.hue.gov.vn/tour-du-lich/danh-sach-tour', '20261022', 1),
('HT-MARKET-BACHMA25', N'Huế – Vườn Quốc gia Bạch Mã – Hải Vọng Đài – Ngũ Hồ – Thác Đỗ Quyên', N'Sinh thái & Đầm phá', N'Tour ghép một ngày từ Huế đến các điểm nổi bật tại Vườn Quốc gia Bạch Mã.', N'Tour được niêm yết trên HueTripO ngày 22/10/2026 với giá từ 1.050.000đ/khách. Giá, lịch, điều kiện thời tiết và dịch vụ cần xác nhận với Công ty CP Du lịch Đại Bàng.', N'Huế -> Vườn Quốc gia Bạch Mã -> Hải Vọng Đài -> Ngũ Hồ -> Thác Đỗ Quyên -> Huế.', N'Vườn Quốc gia Bạch Mã; Hải Vọng Đài; Ngũ Hồ; thác Đỗ Quyên; giá từ 1.050.000đ/khách.', N'1 Ngày', N'Huế', 1050000, N'/images/bach-ma-waterfall.jpg', N'Công ty CP Du lịch Đại Bàng', N'https://huetripo.hue.gov.vn/tour-du-lich/danh-sach-tour', '20261022', 1),
('HT-MARKET-TAMGIANG3D26', N'Hoàng hôn trên phá Tam Giang – Vườn Quốc gia Bạch Mã', N'Sinh thái & Đầm phá', N'Hành trình ghép đoàn ba ngày kết hợp phá Tam Giang và Vườn Quốc gia Bạch Mã.', N'Tour được niêm yết trên HueTripO ngày 22/10/2026 với giá từ 3.990.000đ/khách; điểm đón gồm sân bay Phú Bài, ga Huế hoặc bến xe. Lịch trình cần xác nhận với An Tường Travel.', N'Sân bay Phú Bài / Ga Huế / Bến xe -> Phá Tam Giang -> Vườn Quốc gia Bạch Mã -> Kết thúc theo chương trình An Tường Travel.', N'Phá Tam Giang; Vườn Quốc gia Bạch Mã; tour ghép ba ngày; giá từ 3.990.000đ/khách.', N'3 Ngày', N'Sân bay Phú Bài / Ga Huế / Bến xe', 3990000, N'/images/tam-giang-2024.jpg', N'Công ty TNHH Lữ hành An Tường Travel', N'https://huetripo.hue.gov.vn/tour-du-lich/danh-sach-tour', '20261022', 1),
('HT-MARKET-TAMGIANG2D27', N'Cố đô Huế – Hoàng hôn trên phá Tam Giang – Hoàng cung – Lăng Khải Định – Chùa Thiên Mụ – Chợ Đông Ba', N'Di tích Cố Đô', N'Tour ghép hai ngày một đêm kết hợp di sản Huế và hoàng hôn trên phá Tam Giang.', N'Tour được niêm yết trên HueTripO ngày 22/10/2026 với giá từ 2.890.000đ/khách. Lịch trình từng ngày và dịch vụ cần xác nhận với An Tường Travel.', N'Sân bay Phú Bài / Ga Huế / Bến xe -> Hoàng cung -> Lăng Khải Định -> Chùa Thiên Mụ -> Chợ Đông Ba -> Phá Tam Giang. Lịch chi tiết theo nhà cung cấp.', N'Hoàng cung; lăng Khải Định; chùa Thiên Mụ; chợ Đông Ba; phá Tam Giang; giá từ 2.890.000đ/khách.', N'2 Ngày 1 Đêm', N'Sân bay Phú Bài / Ga Huế / Bến xe', 2890000, N'/images/imperial-city-2025.jpg', N'Công ty TNHH Lữ hành An Tường Travel', N'https://huetripo.hue.gov.vn/tour-du-lich/danh-sach-tour', '20261022', 1),
('HT-MARKET-TAMGIANG-SUNSET28', N'Trải nghiệm trên phá Tam Giang – Ngắm hoàng hôn', N'Sinh thái & Đầm phá', N'Tour ghép nửa ngày khởi hành từ thành phố Huế.', N'Tour được niêm yết trên HueTripO ngày 22/10/2026 với giá từ 850.000đ/khách. Lịch, hoạt động trên phá và chỗ trống cần xác nhận với Restour.', N'Thành phố Huế -> Phá Tam Giang -> Ngắm hoàng hôn -> Trở về Huế. Chi tiết theo chương trình Restour.', N'Phá Tam Giang; ngắm hoàng hôn; tour ghép nửa ngày; giá từ 850.000đ/khách.', N'Nửa Ngày', N'Thành phố Huế', 850000, N'/images/tam-giang-2024.jpg', N'Công ty TNHH MTV Restour', N'https://huetripo.hue.gov.vn/tour-du-lich/danh-sach-tour', '20261022', 1),
('HT-MARKET-SONGHUONG-TEA29', N'Trà chiều trên sông Hương – Du thuyền ngắm hoàng hôn', N'Sông Hương & Làng nghề', N'Trải nghiệm ghép nửa ngày trên sông Hương, thưởng trà và ngắm hoàng hôn.', N'Tour được niêm yết trên HueTripO ngày 22/10/2026 với giá từ 450.000đ/khách. Giờ chạy, nội dung phục vụ và chỗ trống cần xác nhận với Restour.', N'Thành phố Huế -> Du thuyền sông Hương -> Trà chiều và ngắm hoàng hôn -> Kết thúc theo chương trình Restour.', N'Du thuyền sông Hương; trà chiều; ngắm hoàng hôn; giá từ 450.000đ/khách.', N'Nửa Ngày', N'Thành phố Huế', 450000, N'/images/thien-mu-2025.jpg', N'Công ty TNHH MTV Restour', N'https://huetripo.hue.gov.vn/tour-du-lich/danh-sach-tour', '20261022', 1),
('HT-MARKET-THUYBIEU30', N'Khám phá Thủy Biều', N'Sông Hương & Làng nghề', N'Tour ghép nửa ngày khám phá khu vực Thủy Biều, khởi hành từ thành phố Huế.', N'Tour được niêm yết trên HueTripO ngày 22/10/2026 với giá từ 850.000đ/khách. Nội dung và chỗ trống cần xác nhận với Restour.', N'Thành phố Huế -> Khám phá Thủy Biều -> Trở về Huế. Các điểm dừng theo chương trình Restour.', N'Thủy Biều; tour ghép nửa ngày; giá từ 850.000đ/khách.', N'Nửa Ngày', N'Thành phố Huế', 850000, N'/images/thien-mu-2025.jpg', N'Công ty TNHH MTV Restour', N'https://huetripo.hue.gov.vn/tour-du-lich/danh-sach-tour', '20261022', 1),
('HT-MARKET-FOODTOUR31', N'Khám phá ẩm thực xứ Huế – Hue Food Tour', N'Văn hóa & Ẩm thực', N'Tour tự chọn nửa ngày khám phá ẩm thực Huế, khởi hành từ thành phố Huế.', N'Tour được niêm yết trên HueTripO ngày 22/10/2026 với giá từ 850.000đ/khách. Món ăn, điểm dừng và dịch vụ bao gồm cần xác nhận với Restour.', N'Thành phố Huế -> Trải nghiệm ẩm thực theo chương trình Hue Food Tour -> Kết thúc tại Huế.', N'Ẩm thực Huế; tour tự chọn; nửa ngày; giá từ 850.000đ/khách.', N'Nửa Ngày', N'Thành phố Huế', 850000, N'/images/bun-bo-hue-2024.jpg', N'Công ty TNHH MTV Restour', N'https://huetripo.hue.gov.vn/tour-du-lich/danh-sach-tour', '20261022', 1),
('HT-MARKET-VIETRAVEL32', N'Huế – Đại Nội – Lăng Khải Định', N'Di tích Cố Đô', N'Tour ghép một ngày khởi hành tại 01 Hùng Vương, thành phố Huế.', N'Tour được niêm yết trên HueTripO ngày 22/10/2026 với giá từ 890.000đ/khách. Lịch, giá cuối cùng, dịch vụ và chỗ trống cần xác nhận với Vietravel Huế.', N'01 Hùng Vương, TP Huế -> Đại Nội Huế -> Lăng Khải Định -> Kết thúc theo lịch Vietravel Huế.', N'Đại Nội Huế; lăng Khải Định; tour ghép một ngày; giá từ 890.000đ/khách.', N'1 Ngày', N'01 Hùng Vương, TP Huế', 890000, N'/images/imperial-city-2025.jpg', N'Công ty TNHH MTV Du lịch Vietravel Huế', N'https://huetripo.hue.gov.vn/tour-du-lich/danh-sach-tour', '20261022', 1),
('HT-MARKET-BAOVINH33', N'Huế – Phố cổ Bao Vinh – Tranh làng Sình – Làng hoa giấy Thanh Tiên – Bữa trưa dân dã', N'Văn hóa & Ẩm thực', N'Tour ghép nửa ngày kết nối phố cổ Bao Vinh với làng nghề và bữa trưa địa phương.', N'Tour được niêm yết trên HueTripO ngày 22/10/2026 với giá từ 750.000đ/khách. Lịch trình, bữa trưa và chỗ trống cần xác nhận với Công ty CP Du lịch Đại Bàng.', N'Thành phố Huế -> Phố cổ Bao Vinh -> Tranh làng Sình -> Làng hoa giấy Thanh Tiên -> Bữa trưa dân dã. Thứ tự theo nhà cung cấp.', N'Phố cổ Bao Vinh; tranh làng Sình; làng hoa giấy Thanh Tiên; bữa trưa dân dã; giá từ 750.000đ/khách.', N'Nửa Ngày', N'Thành phố Huế', 750000, N'/images/dong-ba-market-2024.jpg', N'Công ty CP Du lịch Đại Bàng', N'https://huetripo.hue.gov.vn/tour-du-lich/danh-sach-tour', '20261022', 1),
('HT-MARKET-HITOUR1D34', N'Tour du lịch Huế trong ngày – Thiên Mụ, Đại Nội, hai lăng và làng nghề', N'Di tích Cố Đô', N'Tour khởi hành hằng ngày từ Huế, tham quan di sản và làng nghề truyền thống.', N'Hitour công bố giá người lớn 850.000đ, áp dụng từ 01/01 đến 31/12/2026; lịch khởi hành hằng ngày. Vui lòng xác nhận giá và chỗ với đơn vị trước khi đặt.', N'Chùa Thiên Mụ; Đại Nội; chợ Đông Ba; lăng Minh Mạng; lăng Khải Định; làng nón; làng hương; giá niêm yết 2026.', N'08:00 Đón tại khách sạn trung tâm Huế -> Chùa Thiên Mụ -> Đại Nội -> Chợ Đông Ba -> Lăng Minh Mạng -> Lăng Khải Định -> Làng nón, làng hương -> 16:30 Trả khách.', N'1 Ngày', N'Trung tâm thành phố Huế', 850000, N'/images/imperial-city-2025.jpg', N'Hitour', N'https://hitour.vn/tour-du-lich-hue/tour-du-lich-hue-1-ngay-tu-thua-thien-hue', '20261022', 1);

-- Nạp lịch khởi hành mẫu
-- Các chuyến không có booking seed bắt đầu với SoChoDaDat = 0 để số ghế luôn khớp vé.

-- Chuyến 1 cho Tour Đại Nội (chuyến đặt mẫu; số chỗ sẽ đồng bộ theo vé phía dưới).
INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (1, DATEADD(DAY, 3, @Now), DATEADD(DAY, 3, DATEADD(HOUR, 9, @Now)), 25, 7, 'Open', N'Cột cờ Phu Văn Lâu', N'Lê Văn Hoàng', '0905111222');
SET @DepartureDaiNoi = CONVERT(INT, SCOPE_IDENTITY());

-- Chuyến 2 cho Tour Đại Nội (không có booking seed; khởi tạo 0 chỗ đã đặt).
INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (1, DATEADD(DAY, 6, @Now), DATEADD(DAY, 6, DATEADD(HOUR, 9, @Now)), 25, 0, 'Open', N'Cột cờ Phu Văn Lâu', N'Nguyễn Thị Thu', '0905333444');

-- Chuyến 3 cho Tour Đại Nội (không có booking seed; không giả lập vé bằng số chỗ đã đặt).
INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (1, DATEADD(DAY, 4, @Now), DATEADD(DAY, 4, DATEADD(HOUR, 9, @Now)), 25, 0, 'Open', N'Cột cờ Phu Văn Lâu', N'Trần Minh Đức', '0905555666');

-- Chuyến 4 cho Tour Đại Nội nằm trong thời hạn khóa đặt để kiểm tra RULE1_CUTOFF_VIOLATION.
INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (1, DATEADD(HOUR, 10, @Now), DATEADD(HOUR, 19, @Now), 25, 0, 'Open', N'Cột cờ Phu Văn Lâu', N'Lê Văn Hoàng', '0905111222');

-- Chuyến cho Tour Đầm Phá Tam Giang
INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (2, DATEADD(DAY, 2, @Now), DATEADD(DAY, 2, DATEADD(HOUR, 6, @Now)), 20, 8, 'Open', N'Bến thuyền Đầm Chuồn', N'Phan Thanh Sang', '0905777888');
SET @DepartureTamGiang = CONVERT(INT, SCOPE_IDENTITY());

INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (2, DATEADD(DAY, 5, @Now), DATEADD(DAY, 5, DATEADD(HOUR, 6, @Now)), 20, 0, 'Open', N'Bến thuyền Đầm Chuồn', N'Phan Thanh Sang', '0905777888');

-- Chuyến cho Tour Du Thuyền Sông Hương Nghe Ca Huế
INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (3, DATEADD(DAY, 2, @Now), DATEADD(DAY, 2, DATEADD(HOUR, 3, @Now)), 30, 14, 'Open', N'Bến thuyền Tòa Khâm', N'Hoàng Yến Nhi', '0905999000');
SET @DepartureSongHuong = CONVERT(INT, SCOPE_IDENTITY());

INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (3, DATEADD(DAY, 4, @Now), DATEADD(DAY, 4, DATEADD(HOUR, 3, @Now)), 30, 0, 'Open', N'Bến thuyền Tòa Khâm', N'Hoàng Yến Nhi', '0905999000');

-- Chuyến cho Tour Thủy Biều Làng Cổ
INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (4, DATEADD(DAY, 3, @Now), DATEADD(DAY, 3, DATEADD(HOUR, 5, @Now)), 15, 0, 'Open', N'Chùa Thiên Mụ', N'Đặng Văn Phú', '0905222111');

-- Chuyến cho Tour Food Tour Đêm Huế
INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (5, DATEADD(DAY, 2, @Now), DATEADD(DAY, 2, DATEADD(HOUR, 4, @Now)), 16, 0, 'Open', N'Cổng Khách sạn Hương Giang', N'Võ Thị Mỹ Linh', '0905444333');

-- Chuyến cho Tour Bạch Mã Trekking
INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (6, DATEADD(DAY, 5, @Now), DATEADD(DAY, 5, DATEADD(HOUR, 10, @Now)), 20, 6, 'Open', N'Trung tâm TP Huế', N'Bùi Quang Dũng', '0905666777');
SET @DepartureBachMa = CONVERT(INT, SCOPE_IDENTITY());

-- Lịch cho các tour khác
INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (7, DATEADD(HOUR, 8, DATEADD(DAY, 4, CONVERT(DATETIME2, CAST(@Now AS DATE)))), DATEADD(MINUTE, 270, DATEADD(HOUR, 8, DATEADD(DAY, 4, CONVERT(DATETIME2, CAST(@Now AS DATE))))), 18, 0, 'Open', N'Trung tâm TP Huế', N'Công ty điều phối', NULL);

INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (8, DATEADD(HOUR, 8, DATEADD(DAY, 3, CONVERT(DATETIME2, CAST(@Now AS DATE)))), DATEADD(HOUR, 13, DATEADD(DAY, 3, CONVERT(DATETIME2, CAST(@Now AS DATE)))), 18, 0, 'Open', N'Trung tâm TP Huế', N'Công ty điều phối', NULL);

INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (9, DATEADD(HOUR, 7, DATEADD(DAY, 5, CONVERT(DATETIME2, CAST(@Now AS DATE)))), DATEADD(HOUR, 17, DATEADD(DAY, 5, CONVERT(DATETIME2, CAST(@Now AS DATE)))), 20, 0, 'Open', N'Trung tâm TP Huế', N'Công ty điều phối', NULL);

-- Chuyến cho Tour 10, 11, 12
INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (10, DATEADD(DAY, 4, @Now), DATEADD(DAY, 4, DATEADD(HOUR, 5, @Now)), 20, 5, 'Open', N'Cột cờ Phu Văn Lâu', N'Phan Thanh Sang', '0905777888');

INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (11, DATEADD(DAY, 3, @Now), DATEADD(DAY, 3, DATEADD(HOUR, 4, @Now)), 16, 4, 'Open', N'Chợ Đông Ba', N'Lê Thị Thu', '0905333444');

INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (12, DATEADD(DAY, 7, @Now), DATEADD(DAY, 7, DATEADD(HOUR, 9, @Now)), 25, 0, 'Closed', N'Trung tâm TP Huế', N'Bùi Quang Dũng', '0905666777');

-- Lịch khởi hành mẫu cho các tour gợi ý bổ sung (13-17)
INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (13, DATEADD(HOUR, 8, DATEADD(DAY, 5, CONVERT(DATETIME2, CAST(@Now AS DATE)))), DATEADD(MINUTE, 510, DATEADD(HOUR, 8, DATEADD(DAY, 5, CONVERT(DATETIME2, CAST(@Now AS DATE))))), 18, 0, 'Open', N'Trung tâm TP Huế', N'Công ty điều phối', NULL);

INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (14, DATEADD(HOUR, 5, DATEADD(DAY, 6, CONVERT(DATETIME2, CAST(@Now AS DATE)))), DATEADD(MINUTE, 330, DATEADD(HOUR, 5, DATEADD(DAY, 6, CONVERT(DATETIME2, CAST(@Now AS DATE))))), 16, 0, 'Open', N'Trung tâm TP Huế', N'Công ty điều phối', NULL);

INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (15, DATEADD(HOUR, 16, DATEADD(DAY, 3, CONVERT(DATETIME2, CAST(@Now AS DATE)))), DATEADD(HOUR, 20, DATEADD(DAY, 3, CONVERT(DATETIME2, CAST(@Now AS DATE)))), 12, 0, 'Open', N'Cầu Trường Tiền', N'Công ty điều phối', NULL);

INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (16, DATEADD(MINUTE, 450, DATEADD(DAY, 8, CONVERT(DATETIME2, CAST(@Now AS DATE)))), DATEADD(MINUTE, 990, DATEADD(DAY, 8, CONVERT(DATETIME2, CAST(@Now AS DATE)))), 18, 0, 'Open', N'Trung tâm TP Huế', N'Công ty điều phối', NULL);

INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (17, DATEADD(HOUR, 8, DATEADD(DAY, 9, CONVERT(DATETIME2, CAST(@Now AS DATE)))), DATEADD(HOUR, 16, DATEADD(DAY, 9, CONVERT(DATETIME2, CAST(@Now AS DATE)))), 18, 0, 'Open', N'Trung tâm TP Huế', N'Công ty điều phối', NULL);

-- Lịch mẫu cho các tour gợi ý bổ sung (18-22); chỉ dùng thử luồng đặt chỗ capstone.
INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES
(18, DATEADD(HOUR, 8, DATEADD(DAY, 6, CONVERT(DATETIME2, CAST(@Now AS DATE)))), DATEADD(MINUTE, 270, DATEADD(HOUR, 8, DATEADD(DAY, 6, CONVERT(DATETIME2, CAST(@Now AS DATE))))), 18, 0, 'Open', N'Trung tâm TP Huế', N'Công ty điều phối', NULL),
(19, DATEADD(MINUTE, 450, DATEADD(DAY, 7, CONVERT(DATETIME2, CAST(@Now AS DATE)))), DATEADD(HOUR, 12, DATEADD(DAY, 7, CONVERT(DATETIME2, CAST(@Now AS DATE)))), 16, 0, 'Open', N'Trung tâm TP Huế', N'Công ty điều phối', NULL),
(20, DATEADD(HOUR, 8, DATEADD(DAY, 8, CONVERT(DATETIME2, CAST(@Now AS DATE)))), DATEADD(HOUR, 15, DATEADD(DAY, 8, CONVERT(DATETIME2, CAST(@Now AS DATE)))), 18, 0, 'Open', N'Trung tâm TP Huế', N'Công ty điều phối', NULL),
(21, DATEADD(HOUR, 8, DATEADD(DAY, 5, CONVERT(DATETIME2, CAST(@Now AS DATE)))), DATEADD(HOUR, 13, DATEADD(DAY, 5, CONVERT(DATETIME2, CAST(@Now AS DATE)))), 16, 0, 'Open', N'Trung tâm TP Huế', N'Công ty điều phối', NULL),
(22, DATEADD(HOUR, 7, DATEADD(DAY, 10, CONVERT(DATETIME2, CAST(@Now AS DATE)))), DATEADD(HOUR, 16, DATEADD(DAY, 10, CONVERT(DATETIME2, CAST(@Now AS DATE)))), 12, 0, 'Open', N'Trung tâm TP Huế', N'Công ty điều phối', NULL);

-- Chuyến lịch sử để đơn Completed có ngày khởi hành hợp lý.
INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (1, DATEADD(DAY, -2, @Now), DATEADD(HOUR, 9, DATEADD(DAY, -2, @Now)), 25, 1, 'Completed', N'Cột cờ Phu Văn Lâu', N'Lê Văn Hoàng', '0905111222');
SET @DepartureLichSu = CONVERT(INT, SCOPE_IDENTITY());

-- Nạp khách hàng mẫu; tài khoản khachhang được liên kết với một hồ sơ tương ứng.
INSERT INTO dbo.Customer (FullName, Email, PhoneNumber, Address, UserAccountId)
VALUES
(N'Trần Minh Tuấn', 'tuan.tran@gmail.com', '0988776655', N'12 Lê Lợi, TP Huế', NULL),
(N'Nguyễn Văn An', 'an.nguyen@gmail.com', '0912345678', N'84 Nguyễn Huệ, TP Huế', (SELECT Id FROM dbo.UserAccount WHERE Username = 'khachhang')),
(N'Nguyễn Hoàng Long', 'long.nguyen@outlook.com', '0903112233', N'158 Phố Huế, Q. Hai Bà Trưng, Hà Nội', NULL),
(N'Vũ Minh Châu', 'minhchau.vu@gmail.com', '0918445566', N'45 Nguyễn Đình Chiểu, Q.1, TP. Hồ Chí Minh', NULL),
(N'Đặng Thu Thảo', 'thuthao.dang@gmail.com', '0935667788', N'22 Bạch Đằng, Q. Hải Châu, Đà Nẵng', NULL),
(N'Hoàng Đức Anh', 'ducanh.hoang@gmail.com', '0983990011', N'08 Hùng Vương, TP Huế', NULL);

-- Nạp đơn đặt tour mẫu. Lấy ID khách theo email thay vì phụ thuộc thứ tự IDENTITY.
-- OUTPUT lưu ID phát sinh để gắn đúng vé hành khách với từng đơn.
DECLARE @BookingIds TABLE
(
    BookingCode NVARCHAR(50) NOT NULL PRIMARY KEY,
    BookingId INT NOT NULL
);

INSERT INTO dbo.DonDatTour (MaDatTour, MaChuyen, MaKhachHang, NgayDat, TongTien, TrangThai, GhiChu, YeuCauDacBiet, PhuongThucThanhToan)
OUTPUT INSERTED.MaDatTour, INSERTED.MaDonDatTour INTO @BookingIds (BookingCode, BookingId)
VALUES
('HUE-202610-001', @DepartureDaiNoi, (SELECT Id FROM dbo.Customer WHERE Email = 'tuan.tran@gmail.com'), DATEADD(DAY, -1, @Now), 825000, 'Confirmed', N'Khách gia đình có 1 bé nhỏ', N'Sắp xếp xe ngồi gần cửa sổ', N'Chuyển khoản QR'),
('HUE-202610-002', @DepartureTamGiang, (SELECT Id FROM dbo.Customer WHERE Email = 'long.nguyen@outlook.com'), DATEADD(DAY, -2, @Now), 1200000, 'Confirmed', N'Đoàn tham gia chèo SUP ngắm hoàng hôn', N'Chuẩn bị áo phao cho trẻ em', N'Chuyển khoản VietQR'),
('HUE-202610-003', @DepartureSongHuong, (SELECT Id FROM dbo.Customer WHERE Email = 'minhchau.vu@gmail.com'), DATEADD(DAY, -1, @Now), 550000, 'Pending', N'Muốn ngồi tầng 2 du thuyền rồng', N'Thả hoa đăng giấy màu', N'Thanh toán trực tiếp'),
('HUE-202610-004', @DepartureBachMa, (SELECT Id FROM dbo.Customer WHERE Email = 'thuthao.dang@gmail.com'), DATEADD(DAY, -3, @Now), 1360000, 'Confirmed', N'Đoàn khám phá Bạch Mã', N'Ăn trưa picnic bên suối', N'Chuyển khoản QR'),
('HUE-202610-005', @DepartureLichSu, (SELECT Id FROM dbo.Customer WHERE Email = 'ducanh.hoang@gmail.com'), DATEADD(DAY, -5, @Now), 550000, 'Completed', N'Khách đi 1 mình công tác', NULL, N'Thẻ tín dụng'),
('HUE-202610-006', @DepartureTamGiang, (SELECT Id FROM dbo.Customer WHERE Email = 'an.nguyen@gmail.com'), DATEADD(DAY, -1, @Now), 480000, 'Cancelled', N'Khách bận việc đột xuất, đã hủy đơn', NULL, N'Chuyển khoản');

-- Nạp vé hành khách mẫu (áp dụng quy tắc giá người lớn, trẻ em và em bé)
-- Booking 1: 1 Người lớn 550k + 1 Trẻ em 275k = 825k
INSERT INTO dbo.PassengerTicket (BookingId, TicketCode, FullName, Gender, PassengerType, BirthYear, Price, SeatNumber)
VALUES
((SELECT BookingId FROM @BookingIds WHERE BookingCode = 'HUE-202610-001'), 'HUE-202610-001-01', N'Trần Minh Tuấn', 'Nam', 'Adult', 1990, 550000, 'G01'),
((SELECT BookingId FROM @BookingIds WHERE BookingCode = 'HUE-202610-001'), 'HUE-202610-001-02', N'Trần Tuấn Khang', 'Nam', 'Child', 2018, 275000, 'G02');

-- Booking 2: 2 Người lớn (480k x 2 = 960k) + 1 Trẻ em (240k) + 1 Em bé (0k) = 1.200.000 VNĐ
INSERT INTO dbo.PassengerTicket (BookingId, TicketCode, FullName, Gender, PassengerType, BirthYear, Price, SeatNumber)
VALUES
((SELECT BookingId FROM @BookingIds WHERE BookingCode = 'HUE-202610-002'), 'HUE-202610-002-01', N'Nguyễn Hoàng Long', 'Nam', 'Adult', 1988, 480000, 'G01'),
((SELECT BookingId FROM @BookingIds WHERE BookingCode = 'HUE-202610-002'), 'HUE-202610-002-02', N'Lê Ngọc Bích', 'Nữ', 'Adult', 1991, 480000, 'G02'),
((SELECT BookingId FROM @BookingIds WHERE BookingCode = 'HUE-202610-002'), 'HUE-202610-002-03', N'Nguyễn Hoàng Nam', 'Nam', 'Child', 2016, 240000, 'G03'),
((SELECT BookingId FROM @BookingIds WHERE BookingCode = 'HUE-202610-002'), 'HUE-202610-002-04', N'Nguyễn Ngọc An', 'Nữ', 'Infant', 2023, 0, 'G04');

-- Booking 3: 2 Người lớn (220k x 2 = 440k) + 1 Trẻ em (110k) = 550.000 VNĐ
INSERT INTO dbo.PassengerTicket (BookingId, TicketCode, FullName, Gender, PassengerType, BirthYear, Price, SeatNumber)
VALUES
((SELECT BookingId FROM @BookingIds WHERE BookingCode = 'HUE-202610-003'), 'HUE-202610-003-01', N'Vũ Minh Châu', 'Nữ', 'Adult', 1995, 220000, 'G05'),
((SELECT BookingId FROM @BookingIds WHERE BookingCode = 'HUE-202610-003'), 'HUE-202610-003-02', N'Phạm Nhật Minh', 'Nam', 'Adult', 1993, 220000, 'G06'),
((SELECT BookingId FROM @BookingIds WHERE BookingCode = 'HUE-202610-003'), 'HUE-202610-003-03', N'Phạm Minh Nhi', 'Nữ', 'Child', 2019, 110000, 'G07');

-- Booking 4: 2 Người lớn (680k x 2) = 1.360.000 VNĐ
INSERT INTO dbo.PassengerTicket (BookingId, TicketCode, FullName, Gender, PassengerType, BirthYear, Price, SeatNumber)
VALUES
((SELECT BookingId FROM @BookingIds WHERE BookingCode = 'HUE-202610-004'), 'HUE-202610-004-01', N'Đặng Thu Thảo', 'Nữ', 'Adult', 1997, 680000, 'G01'),
((SELECT BookingId FROM @BookingIds WHERE BookingCode = 'HUE-202610-004'), 'HUE-202610-004-02', N'Nguyễn Quang Vinh', 'Nam', 'Adult', 1996, 680000, 'G02');

-- Booking 5: 1 Người lớn = 550.000 VNĐ, đi trên chuyến đã hoàn thành
INSERT INTO dbo.PassengerTicket (BookingId, TicketCode, FullName, Gender, PassengerType, BirthYear, Price, SeatNumber)
VALUES
((SELECT BookingId FROM @BookingIds WHERE BookingCode = 'HUE-202610-005'), 'HUE-202610-005-01', N'Hoàng Đức Anh', 'Nam', 'Adult', 1992, 550000, 'G08');

-- Booking 6: 1 Người lớn = 480.000 VNĐ (đã hủy)
INSERT INTO dbo.PassengerTicket (BookingId, TicketCode, FullName, Gender, PassengerType, BirthYear, Price, SeatNumber)
VALUES
((SELECT BookingId FROM @BookingIds WHERE BookingCode = 'HUE-202610-006'), 'HUE-202610-006-01', N'Lê Thị Mai Anh', 'Nữ', 'Adult', 1994, 480000, 'G09');

-- Đồng bộ số chỗ đã đặt theo các vé chưa bị hủy của những chuyến có đơn mẫu.
UPDATE d
SET d.BookedSeats =
(
    SELECT COUNT(*)
    FROM dbo.TourBooking AS b
    INNER JOIN dbo.PassengerTicket AS pt ON pt.BookingId = b.Id
    WHERE b.DepartureId = d.Id
      AND b.Status <> 'Cancelled'
)
FROM dbo.Departure AS d
WHERE d.Id IN (@DepartureDaiNoi, @DepartureTamGiang, @DepartureSongHuong, @DepartureBachMa, @DepartureLichSu);

-- Kiểm tra đơn nào cũng có vé và tổng tiền khớp tổng giá vé.
IF EXISTS
(
    SELECT b.Id
    FROM dbo.TourBooking AS b
    LEFT JOIN dbo.PassengerTicket AS pt ON pt.BookingId = b.Id
    GROUP BY b.Id, b.TotalAmount
    HAVING COUNT(pt.Id) = 0 OR SUM(COALESCE(pt.Price, 0)) <> b.TotalAmount
)
BEGIN
    ;THROW 51001, N'Dữ liệu mẫu không hợp lệ: đơn thiếu vé hoặc tổng tiền không khớp.', 1;
END;

-- Kiểm tra giá từng vé đúng với giá tour và hệ số theo loại hành khách.
IF EXISTS
(
    SELECT 1
    FROM dbo.PassengerTicket AS pt
    INNER JOIN dbo.TourBooking AS b ON b.Id = pt.BookingId
    INNER JOIN dbo.Departure AS d ON d.Id = b.DepartureId
    INNER JOIN dbo.Tour AS t ON t.Id = d.TourId
    INNER JOIN dbo.PriceRule AS pr ON pr.PassengerType = pt.PassengerType
    WHERE pt.Price <> CONVERT(DECIMAL(18,2), t.BasePrice * pr.RateMultiplier)
)
BEGIN
    ;THROW 51002, N'Dữ liệu mẫu không hợp lệ: giá vé không khớp giá tour hoặc loại hành khách.', 1;
END;

-- Kiểm tra số chỗ của các chuyến có đơn mẫu khớp với số vé chưa bị hủy.
IF EXISTS
(
    SELECT 1
    FROM dbo.Departure AS d
    WHERE d.Id IN (@DepartureDaiNoi, @DepartureTamGiang, @DepartureSongHuong, @DepartureBachMa, @DepartureLichSu)
      AND d.BookedSeats <>
      (
          SELECT COUNT(*)
          FROM dbo.TourBooking AS b
          INNER JOIN dbo.PassengerTicket AS pt ON pt.BookingId = b.Id
          WHERE b.DepartureId = d.Id
            AND b.Status <> 'Cancelled'
      )
)
BEGIN
    ;THROW 51003, N'Dữ liệu mẫu không hợp lệ: số chỗ đã đặt không khớp số vé còn hiệu lực.', 1;
END;

COMMIT TRANSACTION;

-- Bảng tổng hợp đơn đặt tour để xem ngay trong SQL Server Management Studio.
SELECT
    b.BookingCode AS [Mã đặt tour],
    c.FullName AS [Khách hàng],
    t.Title AS [Tour],
    d.DepartureDate AS [Ngày khởi hành],
    CASE b.Status
        WHEN 'Pending' THEN N'Chờ xác nhận'
        WHEN 'Confirmed' THEN N'Đã xác nhận'
        WHEN 'Completed' THEN N'Đã hoàn thành'
        WHEN 'Cancelled' THEN N'Đã hủy'
        ELSE b.Status
    END AS [Trạng thái],
    COUNT(pt.Id) AS [Số hành khách],
    b.TotalAmount AS [Tổng tiền (VNĐ)]
FROM dbo.TourBooking AS b
INNER JOIN dbo.Customer AS c ON c.Id = b.CustomerId
INNER JOIN dbo.Departure AS d ON d.Id = b.DepartureId
INNER JOIN dbo.Tour AS t ON t.Id = d.TourId
INNER JOIN dbo.PassengerTicket AS pt ON pt.BookingId = b.Id
GROUP BY b.BookingCode, c.FullName, t.Title, d.DepartureDate, b.Status, b.TotalAmount, b.BookingDate
ORDER BY b.BookingDate, b.BookingCode;

-- Danh sách chi tiết từng hành khách và giá vé.
SELECT
    b.BookingCode AS [Mã đặt tour],
    CASE b.Status
        WHEN 'Pending' THEN N'Chờ xác nhận'
        WHEN 'Confirmed' THEN N'Đã xác nhận'
        WHEN 'Completed' THEN N'Đã hoàn thành'
        WHEN 'Cancelled' THEN N'Đã hủy'
        ELSE b.Status
    END AS [Trạng thái],
    pt.FullName AS [Hành khách],
    pr.DisplayName AS [Loại vé],
    pt.TicketCode AS [Mã vé],
    pt.Price AS [Giá vé (VNĐ)]
FROM dbo.PassengerTicket AS pt
INNER JOIN dbo.TourBooking AS b ON b.Id = pt.BookingId
INNER JOIN dbo.PriceRule AS pr ON pr.PassengerType = pt.PassengerType
ORDER BY b.BookingCode, pt.TicketCode;

    PRINT N'Cơ sở dữ liệu HueTourDb đã được khởi tạo và nạp dữ liệu mẫu hoàn tất thành công!';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;

GO

