-- Đưa 11 tour tham khảo về đúng dải ID 1002–1012, giữ nguyên toàn bộ nội dung.
-- An toàn khi chạy lại. Dừng nếu ID đích đã thuộc tour khác hoặc tour đang có chuyến.
SET XACT_ABORT ON;

DECLARE @MarketMap TABLE (
    TargetId INT NOT NULL PRIMARY KEY,
    Code NVARCHAR(50) NOT NULL UNIQUE
);

INSERT INTO @MarketMap (TargetId, Code) VALUES
    (1002, N'HT-MARKET-THANHTAN24'),
    (1003, N'HT-MARKET-BACHMA25'),
    (1004, N'HT-MARKET-TAMGIANG3D26'),
    (1005, N'HT-MARKET-TAMGIANG2D27'),
    (1006, N'HT-MARKET-TAMGIANG-SUNSET28'),
    (1007, N'HT-MARKET-SONGHUONG-TEA29'),
    (1008, N'HT-MARKET-THUYBIEU30'),
    (1009, N'HT-MARKET-FOODTOUR31'),
    (1010, N'HT-MARKET-VIETRAVEL32'),
    (1011, N'HT-MARKET-BAOVINH33'),
    (1012, N'HT-MARKET-HITOUR1D34');

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.TourDuLich', N'U') IS NULL
        THROW 51010, N'Không tìm thấy dbo.TourDuLich. Hãy tạo database trước.', 1;

    IF EXISTS (
        SELECT 1
        FROM @MarketMap AS map
        INNER JOIN dbo.TourDuLich AS occupied ON occupied.MaTour = map.TargetId
        WHERE occupied.MaTourHienThi <> map.Code
    )
        THROW 51011, N'Một ID trong dải 1002–1012 đang thuộc tour khác; không thay đổi dữ liệu.', 1;

    IF EXISTS (
        SELECT 1
        FROM @MarketMap AS map
        INNER JOIN dbo.TourDuLich AS tour ON tour.MaTourHienThi = map.Code
        INNER JOIN dbo.ChuyenKhoiHanh AS departure ON departure.MaTour = tour.MaTour
    )
        THROW 51012, N'Tour thị trường đang có chuyến khởi hành; migration không thay đổi dữ liệu.', 1;

    SELECT map.TargetId,
           tour.MaTour AS OldId,
           tour.MaTourHienThi,
           tour.TenTour,
           tour.DanhMuc,
           tour.MoTaNgan,
           tour.MoTaChiTiet,
           tour.LichTrinh,
           tour.DiemNoiBat,
           tour.ThoiLuong,
           tour.DiemKhoiHanh,
           tour.GiaCoBan,
           tour.DuongDanAnh,
           tour.DonViToChuc,
           tour.NguonGia,
           tour.NgayKiemTraGia,
           tour.DangMoBan,
           tour.NgayTao
    INTO #MarketTourIdMoves
    FROM @MarketMap AS map
    INNER JOIN dbo.TourDuLich AS tour ON tour.MaTourHienThi = map.Code
    WHERE tour.MaTour <> map.TargetId;

    DELETE tour
    FROM dbo.TourDuLich AS tour
    INNER JOIN #MarketTourIdMoves AS move ON move.OldId = tour.MaTour;

    IF EXISTS (SELECT 1 FROM #MarketTourIdMoves)
    BEGIN
        SET IDENTITY_INSERT dbo.TourDuLich ON;

        INSERT INTO dbo.TourDuLich (
            MaTour, MaTourHienThi, TenTour, DanhMuc, MoTaNgan, MoTaChiTiet, LichTrinh,
            DiemNoiBat, ThoiLuong, DiemKhoiHanh, GiaCoBan, DuongDanAnh, DonViToChuc,
            NguonGia, NgayKiemTraGia, DangMoBan, NgayTao
        )
        SELECT TargetId, MaTourHienThi, TenTour, DanhMuc, MoTaNgan, MoTaChiTiet, LichTrinh,
               DiemNoiBat, ThoiLuong, DiemKhoiHanh, GiaCoBan, DuongDanAnh, DonViToChuc,
               NguonGia, NgayKiemTraGia, DangMoBan, NgayTao
        FROM #MarketTourIdMoves;

        SET IDENTITY_INSERT dbo.TourDuLich OFF;
    END;

    IF EXISTS (
        SELECT 1
        FROM @MarketMap AS map
        LEFT JOIN dbo.TourDuLich AS tour
            ON tour.MaTour = map.TargetId AND tour.MaTourHienThi = map.Code
        WHERE tour.MaTour IS NULL
    )
        THROW 51013, N'Chưa đủ 11 tour thị trường; hãy nạp seed trước khi căn chỉnh ID.', 1;

    IF EXISTS (
        SELECT 1
        FROM @MarketMap AS map
        INNER JOIN dbo.TourDuLich AS tour
            ON tour.MaTour = map.TargetId AND tour.MaTourHienThi = map.Code
        WHERE tour.DonViToChuc IS NULL OR tour.NguonGia IS NULL
           OR tour.NgayKiemTraGia IS NULL OR tour.DangMoBan = 0
    )
        THROW 51014, N'Tour thị trường còn thiếu nhà cung cấp, nguồn giá hoặc ngày kiểm tra.', 1;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF OBJECTPROPERTY(OBJECT_ID(N'dbo.TourDuLich'), N'TableHasIdentity') = 1
        SET IDENTITY_INSERT dbo.TourDuLich OFF;
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

SELECT map.TargetId AS Id, map.Code, tour.TenTour AS Title,
       tour.DonViToChuc AS ProviderName, tour.NguonGia AS SourceUrl,
       tour.NgayKiemTraGia AS PriceVerifiedAt, tour.DangMoBan AS IsActive
FROM @MarketMap AS map
LEFT JOIN dbo.TourDuLich AS tour ON tour.MaTour = map.TargetId
ORDER BY map.TargetId;
