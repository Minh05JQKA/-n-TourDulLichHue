-- Chuẩn hóa danh mục ẩm thực cho các database đã được tạo bằng schema cũ.
-- Sao chép ánh xạ hiện có trước khi bỏ cột tên danh mục lặp ở từng địa điểm.
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.DiaDiemAmThuc', N'U') IS NULL
    THROW 51001, N'Không tìm thấy dbo.DiaDiemAmThuc. Hãy tạo database trước.', 1;

IF COL_LENGTH(N'dbo.DiaDiemAmThuc', N'DanhMuc') IS NOT NULL
BEGIN
    EXEC sys.sp_executesql N'
        IF EXISTS (
            SELECT MaDanhMuc
            FROM dbo.DiaDiemAmThuc
            GROUP BY MaDanhMuc
            HAVING COUNT(DISTINCT DanhMuc) > 1
        )
            THROW 51002, N''Một mã danh mục đang trỏ tới nhiều tên; cần rà dữ liệu trước khi chuẩn hóa.'', 1;
    ';
END;

IF OBJECT_ID(N'dbo.DanhMucAmThuc', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DanhMucAmThuc (
        MaDanhMuc NVARCHAR(50) NOT NULL CONSTRAINT PK_DanhMucAmThuc PRIMARY KEY,
        TenDanhMuc NVARCHAR(80) NOT NULL CONSTRAINT UQ_DanhMucAmThuc_Ten UNIQUE
    );
END;

IF COL_LENGTH(N'dbo.DiaDiemAmThuc', N'DanhMuc') IS NOT NULL
BEGIN
    EXEC sys.sp_executesql N'
        INSERT INTO dbo.DanhMucAmThuc (MaDanhMuc, TenDanhMuc)
        SELECT place.MaDanhMuc, MIN(place.DanhMuc)
        FROM dbo.DiaDiemAmThuc AS place
        WHERE NOT EXISTS (
            SELECT 1
            FROM dbo.DanhMucAmThuc AS category
            WHERE category.MaDanhMuc = place.MaDanhMuc
        )
        GROUP BY place.MaDanhMuc;

        IF EXISTS (
            SELECT 1
            FROM dbo.DiaDiemAmThuc AS place
            LEFT JOIN dbo.DanhMucAmThuc AS category ON category.MaDanhMuc = place.MaDanhMuc
            WHERE category.MaDanhMuc IS NULL OR category.TenDanhMuc <> place.DanhMuc
        )
            THROW 51003, N''Ánh xạ danh mục không nhất quán; chưa thay đổi cấu trúc bảng địa điểm.'', 1;
    ';
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_DiaDiemAmThuc_DanhMucAmThuc'
      AND parent_object_id = OBJECT_ID(N'dbo.DiaDiemAmThuc')
)
BEGIN
    ALTER TABLE dbo.DiaDiemAmThuc WITH CHECK
        ADD CONSTRAINT FK_DiaDiemAmThuc_DanhMucAmThuc
        FOREIGN KEY (MaDanhMuc) REFERENCES dbo.DanhMucAmThuc(MaDanhMuc);
END;

IF COL_LENGTH(N'dbo.DiaDiemAmThuc', N'DanhMuc') IS NOT NULL
    ALTER TABLE dbo.DiaDiemAmThuc DROP COLUMN DanhMuc;

COMMIT TRANSACTION;
