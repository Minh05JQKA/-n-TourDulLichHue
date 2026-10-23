using System.Data;
using Dapper;
using HueTour.CoreBusiness;
using HueTour.UseCases.Common;
using HueTour.UseCases.FoodPlaces;
using HueTour.UseCases.PluginInterfaces.DataStore;

namespace HueTour.Plugins.DataStore.SQL;

/// <summary>
/// Kho địa điểm ẩm thực lưu trên SQL Server (bảng dbo.DiaDiemAmThuc).
/// Tên danh mục được đọc từ bảng danh mục chuẩn hóa.
/// </summary>
public class FoodPlaceRepository : IFoodPlaceRepository
{
    private readonly ISqlDbConnectionFactory _connectionFactory;

    public FoodPlaceRepository(ISqlDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<FoodPlace>> GetAllAsync()
    {
        using IDbConnection conn = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT p.MaDiaDiem AS Id, p.Slug, p.TenDiaDiem AS Name,
                   category.TenDanhMuc AS Category, p.MaDanhMuc AS CategoryCode,
                   p.MonDacTrung AS SignatureDish, p.MoTa AS Description,
                   p.DiaChi AS Address, p.PhuongXa AS Ward,
                   CAST(p.ToaDoLat AS FLOAT) AS Latitude, CAST(p.ToaDoLng AS FLOAT) AS Longitude,
                   p.SoDienThoai AS Phone, p.GiaThamKhao AS AveragePrice,
                   CAST(p.DiemDanhGia AS FLOAT) AS Rating, p.SoDanhGia AS ReviewCount,
                   p.AnhMinhHoa AS ImageUrl, p.AnhGoc AS SourceImageUrl,
                   p.Nguon AS Source, p.DuocDoiChieu AS IsCurated
            FROM dbo.DiaDiemAmThuc AS p
            INNER JOIN dbo.DanhMucAmThuc AS category ON category.MaDanhMuc = p.MaDanhMuc
            ORDER BY p.MaDiaDiem;
            """;
        return await conn.QueryAsync<FoodPlace>(sql);
    }

    public async Task<FoodPlace?> GetByIdAsync(int id)
    {
        using IDbConnection conn = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT p.MaDiaDiem AS Id, p.Slug, p.TenDiaDiem AS Name,
                   category.TenDanhMuc AS Category, p.MaDanhMuc AS CategoryCode,
                   p.MonDacTrung AS SignatureDish, p.MoTa AS Description,
                   p.DiaChi AS Address, p.PhuongXa AS Ward,
                   CAST(p.ToaDoLat AS FLOAT) AS Latitude, CAST(p.ToaDoLng AS FLOAT) AS Longitude,
                   p.SoDienThoai AS Phone, p.GiaThamKhao AS AveragePrice,
                   CAST(p.DiemDanhGia AS FLOAT) AS Rating, p.SoDanhGia AS ReviewCount,
                   p.AnhMinhHoa AS ImageUrl, p.AnhGoc AS SourceImageUrl,
                   p.Nguon AS Source, p.DuocDoiChieu AS IsCurated
            FROM dbo.DiaDiemAmThuc AS p
            INNER JOIN dbo.DanhMucAmThuc AS category ON category.MaDanhMuc = p.MaDanhMuc
            WHERE p.MaDiaDiem = @Id;
            """;
        return await conn.QuerySingleOrDefaultAsync<FoodPlace>(sql, new { Id = id });
    }

    public async Task<FoodPlace?> GetBySlugAsync(string slug)
    {
        using IDbConnection conn = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT p.MaDiaDiem AS Id, p.Slug, p.TenDiaDiem AS Name,
                   category.TenDanhMuc AS Category, p.MaDanhMuc AS CategoryCode,
                   p.MonDacTrung AS SignatureDish, p.MoTa AS Description,
                   p.DiaChi AS Address, p.PhuongXa AS Ward,
                   CAST(p.ToaDoLat AS FLOAT) AS Latitude, CAST(p.ToaDoLng AS FLOAT) AS Longitude,
                   p.SoDienThoai AS Phone, p.GiaThamKhao AS AveragePrice,
                   CAST(p.DiemDanhGia AS FLOAT) AS Rating, p.SoDanhGia AS ReviewCount,
                   p.AnhMinhHoa AS ImageUrl, p.AnhGoc AS SourceImageUrl,
                   p.Nguon AS Source, p.DuocDoiChieu AS IsCurated
            FROM dbo.DiaDiemAmThuc AS p
            INNER JOIN dbo.DanhMucAmThuc AS category ON category.MaDanhMuc = p.MaDanhMuc
            WHERE p.Slug = @Slug;
            """;
        return await conn.QuerySingleOrDefaultAsync<FoodPlace>(sql, new { Slug = slug });
    }

    public async Task<IEnumerable<FoodPlace>> SearchAsync(string? keyword, string? categoryCode, GeoViewport? bounds = null)
    {
        using IDbConnection conn = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT p.MaDiaDiem AS Id, p.Slug, p.TenDiaDiem AS Name,
                   category.TenDanhMuc AS Category, p.MaDanhMuc AS CategoryCode,
                   p.MonDacTrung AS SignatureDish, p.MoTa AS Description,
                   p.DiaChi AS Address, p.PhuongXa AS Ward,
                   CAST(p.ToaDoLat AS FLOAT) AS Latitude, CAST(p.ToaDoLng AS FLOAT) AS Longitude,
                   p.SoDienThoai AS Phone, p.GiaThamKhao AS AveragePrice,
                   CAST(p.DiemDanhGia AS FLOAT) AS Rating, p.SoDanhGia AS ReviewCount,
                   p.AnhMinhHoa AS ImageUrl, p.AnhGoc AS SourceImageUrl,
                   p.Nguon AS Source, p.DuocDoiChieu AS IsCurated
            FROM dbo.DiaDiemAmThuc AS p
            INNER JOIN dbo.DanhMucAmThuc AS category ON category.MaDanhMuc = p.MaDanhMuc
            WHERE (@CategoryCode IS NULL OR p.MaDanhMuc = @CategoryCode)
              AND (@BoundsWest IS NULL OR p.ToaDoLng >= @BoundsWest)
              AND (@BoundsEast IS NULL OR p.ToaDoLng <= @BoundsEast)
              AND (@BoundsSouth IS NULL OR p.ToaDoLat >= @BoundsSouth)
              AND (@BoundsNorth IS NULL OR p.ToaDoLat <= @BoundsNorth)
              AND (
                    @Keyword IS NULL
                 OR p.TenDiaDiem LIKE @Pattern
                 OR p.DiaChi LIKE @Pattern
                 OR p.PhuongXa LIKE @Pattern
                 OR p.MonDacTrung LIKE @Pattern
                 OR category.TenDanhMuc LIKE @Pattern
              )
            ORDER BY p.DuocDoiChieu DESC, p.DiemDanhGia DESC, p.MaDiaDiem;
            """;

        var pattern = string.IsNullOrWhiteSpace(keyword) ? null : $"%{keyword.Trim()}%";

        return await conn.QueryAsync<FoodPlace>(sql, new
        {
            CategoryCode = string.IsNullOrWhiteSpace(categoryCode) ? null : categoryCode.Trim(),
            Keyword = string.IsNullOrWhiteSpace(keyword) ? null : keyword.Trim(),
            Pattern = pattern,
            BoundsWest = bounds?.West,
            BoundsSouth = bounds?.South,
            BoundsEast = bounds?.East,
            BoundsNorth = bounds?.North
        });
    }

    public async Task<IReadOnlyList<CategorySummary>> GetCategoriesAsync()
    {
        using IDbConnection conn = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT category.MaDanhMuc AS CategoryCode,
                   category.TenDanhMuc AS DisplayName,
                   COUNT(*) AS Count
            FROM dbo.DiaDiemAmThuc AS p
            INNER JOIN dbo.DanhMucAmThuc AS category ON category.MaDanhMuc = p.MaDanhMuc
            GROUP BY category.MaDanhMuc, category.TenDanhMuc
            ORDER BY COUNT(*) DESC;
            """;
        var rows = await conn.QueryAsync<CategorySummary>(sql);
        return rows.ToList();
    }

    public async Task<IEnumerable<FoodPlace>> GetFeaturedAsync(int take = 6, string? categoryCode = null)
    {
        using IDbConnection conn = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT TOP (@Take)
                   p.MaDiaDiem AS Id, p.Slug, p.TenDiaDiem AS Name,
                   category.TenDanhMuc AS Category, p.MaDanhMuc AS CategoryCode,
                   p.MonDacTrung AS SignatureDish, p.MoTa AS Description,
                   p.DiaChi AS Address, p.PhuongXa AS Ward,
                   CAST(p.ToaDoLat AS FLOAT) AS Latitude, CAST(p.ToaDoLng AS FLOAT) AS Longitude,
                   p.SoDienThoai AS Phone, p.GiaThamKhao AS AveragePrice,
                   CAST(p.DiemDanhGia AS FLOAT) AS Rating, p.SoDanhGia AS ReviewCount,
                   p.AnhMinhHoa AS ImageUrl, p.AnhGoc AS SourceImageUrl,
                   p.Nguon AS Source, p.DuocDoiChieu AS IsCurated
            FROM dbo.DiaDiemAmThuc AS p
            INNER JOIN dbo.DanhMucAmThuc AS category ON category.MaDanhMuc = p.MaDanhMuc
            WHERE (@CategoryCode IS NULL OR p.MaDanhMuc = @CategoryCode)
            ORDER BY p.DuocDoiChieu DESC, p.DiemDanhGia DESC, p.SoDanhGia DESC, p.MaDiaDiem;
            """;
        return await conn.QueryAsync<FoodPlace>(sql, new
        {
            Take = Math.Max(1, take),
            CategoryCode = string.IsNullOrWhiteSpace(categoryCode) ? null : categoryCode.Trim()
        });
    }
}
