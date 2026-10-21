using System.Data;
using Dapper;
using HueTour.CoreBusiness;
using HueTour.UseCases.FoodPlaces;
using HueTour.UseCases.PluginInterfaces.DataStore;

namespace HueTour.Plugins.DataStore.SQL;

/// <summary>
/// Kho địa điểm ẩm thực lưu trên SQL Server (bảng dbo.DiaDiemAmThuc).
/// Dữ liệu được nạp bằng tệp database/seed/DiaDiemAmThuc_Seed.sql.
/// </summary>
public class FoodPlaceRepository : IFoodPlaceRepository
{
    private const string SelectColumns = """
        MaDiaDiem AS Id, Slug, TenDiaDiem AS Name, DanhMuc AS Category, MaDanhMuc AS CategoryCode,
        MonDacTrung AS SignatureDish, MoTa AS Description, DiaChi AS Address, PhuongXa AS Ward,
        CAST(ToaDoLat AS FLOAT) AS Latitude, CAST(ToaDoLng AS FLOAT) AS Longitude, SoDienThoai AS Phone,
        GiaThamKhao AS AveragePrice, CAST(DiemDanhGia AS FLOAT) AS Rating, SoDanhGia AS ReviewCount,
        AnhMinhHoa AS ImageUrl, AnhGoc AS SourceImageUrl, Nguon AS Source, DuocDoiChieu AS IsCurated
        """;

    private readonly ISqlDbConnectionFactory _connectionFactory;

    public FoodPlaceRepository(ISqlDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<FoodPlace>> GetAllAsync()
    {
        using IDbConnection conn = _connectionFactory.CreateConnection();
        const string sql = $"""
            SELECT {SelectColumns}
            FROM dbo.DiaDiemAmThuc
            ORDER BY MaDiaDiem;
            """;
        return await conn.QueryAsync<FoodPlace>(sql);
    }

    public async Task<FoodPlace?> GetByIdAsync(int id)
    {
        using IDbConnection conn = _connectionFactory.CreateConnection();
        const string sql = $"""
            SELECT {SelectColumns}
            FROM dbo.DiaDiemAmThuc
            WHERE MaDiaDiem = @Id;
            """;
        return await conn.QuerySingleOrDefaultAsync<FoodPlace>(sql, new { Id = id });
    }

    public async Task<FoodPlace?> GetBySlugAsync(string slug)
    {
        using IDbConnection conn = _connectionFactory.CreateConnection();
        const string sql = $"""
            SELECT {SelectColumns}
            FROM dbo.DiaDiemAmThuc
            WHERE Slug = @Slug;
            """;
        return await conn.QuerySingleOrDefaultAsync<FoodPlace>(sql, new { Slug = slug });
    }

    public async Task<IEnumerable<FoodPlace>> SearchAsync(string? keyword, string? categoryCode, GeoViewport? bounds = null)
    {
        using IDbConnection conn = _connectionFactory.CreateConnection();
        const string sql = $"""
            SELECT {SelectColumns}
            FROM dbo.DiaDiemAmThuc
            WHERE (@CategoryCode IS NULL OR MaDanhMuc = @CategoryCode)
              AND (@BoundsWest IS NULL OR ToaDoLng >= @BoundsWest)
              AND (@BoundsEast IS NULL OR ToaDoLng <= @BoundsEast)
              AND (@BoundsSouth IS NULL OR ToaDoLat >= @BoundsSouth)
              AND (@BoundsNorth IS NULL OR ToaDoLat <= @BoundsNorth)
              AND (
                    @Keyword IS NULL
                 OR TenDiaDiem LIKE @Pattern
                 OR DiaChi LIKE @Pattern
                 OR PhuongXa LIKE @Pattern
                 OR MonDacTrung LIKE @Pattern
                 OR DanhMuc LIKE @Pattern
              )
            ORDER BY DuocDoiChieu DESC, DiemDanhGia DESC, MaDiaDiem;
            """;

        var pattern = string.IsNullOrWhiteSpace(keyword)
            ? null
            : $"%{keyword.Trim()}%";

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
            SELECT MaDanhMuc AS CategoryCode, DanhMuc AS DisplayName, COUNT(*) AS Count
            FROM dbo.DiaDiemAmThuc
            GROUP BY MaDanhMuc, DanhMuc
            ORDER BY COUNT(*) DESC;
            """;
        var rows = await conn.QueryAsync<CategorySummary>(sql);
        return rows.ToList();
    }

    public async Task<IEnumerable<FoodPlace>> GetFeaturedAsync(int take = 6, string? categoryCode = null)
    {
        using IDbConnection conn = _connectionFactory.CreateConnection();
        const string sql = $"""
            SELECT TOP (@Take) {SelectColumns}
            FROM dbo.DiaDiemAmThuc
            WHERE (@CategoryCode IS NULL OR MaDanhMuc = @CategoryCode)
            ORDER BY DuocDoiChieu DESC, DiemDanhGia DESC, SoDanhGia DESC, MaDiaDiem;
            """;
        return await conn.QueryAsync<FoodPlace>(sql, new
        {
            Take = Math.Max(1, take),
            CategoryCode = string.IsNullOrWhiteSpace(categoryCode) ? null : categoryCode.Trim()
        });
    }
}
