using System.Data;
using Dapper;
using HueTour.CoreBusiness;
using HueTour.UseCases.PluginInterfaces.DataStore;

namespace HueTour.Plugins.DataStore.SQL;

public class TourRepository : ITourRepository
{
    private readonly ISqlDbConnectionFactory _connectionFactory;

    public TourRepository(ISqlDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Tour>> GetAllToursAsync(bool activeOnly = true)
    {
        using IDbConnection conn = _connectionFactory.CreateConnection();
        string sql = activeOnly
            ? "SELECT * FROM dbo.Tour WHERE IsActive = 1 ORDER BY Id DESC"
            : "SELECT * FROM dbo.Tour ORDER BY Id DESC";

        return await conn.QueryAsync<Tour>(sql);
    }

    public async Task<Tour?> GetTourByIdAsync(int id)
    {
        using IDbConnection conn = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM dbo.Tour WHERE Id = @Id";
        return await conn.QuerySingleOrDefaultAsync<Tour>(sql, new { Id = id });
    }

    public async Task<Tour?> GetTourByCodeAsync(string code)
    {
        using IDbConnection conn = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM dbo.Tour WHERE Code = @Code";
        return await conn.QuerySingleOrDefaultAsync<Tour>(sql, new { Code = code });
    }

    public async Task<IEnumerable<Tour>> SearchToursAsync(string? searchTerm, string? category)
    {
        using IDbConnection conn = _connectionFactory.CreateConnection();
        var sql = "SELECT * FROM dbo.Tour WHERE IsActive = 1";
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            sql += " AND (Title LIKE @SearchPattern OR ShortDescription LIKE @SearchPattern OR Highlights LIKE @SearchPattern)";
            parameters.Add("SearchPattern", $"%{searchTerm.Trim()}%");
        }

        if (!string.IsNullOrWhiteSpace(category) && category != "Tất cả")
        {
            sql += " AND Category = @Category";
            parameters.Add("Category", category.Trim());
        }

        sql += " ORDER BY Id DESC";
        return await conn.QueryAsync<Tour>(sql, parameters);
    }

    public async Task<int> AddTourAsync(Tour tour)
    {
        using IDbConnection conn = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO dbo.Tour (Code, Title, Category, ShortDescription, Description, Itinerary, Highlights, Duration, StartingLocation, BasePrice, ImageUrl, IsActive, CreatedAt)
            VALUES (@Code, @Title, @Category, @ShortDescription, @Description, @Itinerary, @Highlights, @Duration, @StartingLocation, @BasePrice, @ImageUrl, @IsActive, @CreatedAt);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        return await conn.ExecuteScalarAsync<int>(sql, tour);
    }

    public async Task UpdateTourAsync(Tour tour)
    {
        using IDbConnection conn = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE dbo.Tour
            SET Code = @Code, Title = @Title, Category = @Category, ShortDescription = @ShortDescription,
                Description = @Description, Itinerary = @Itinerary, Highlights = @Highlights,
                Duration = @Duration, StartingLocation = @StartingLocation, BasePrice = @BasePrice,
                ImageUrl = @ImageUrl, IsActive = @IsActive
            WHERE Id = @Id";

        await conn.ExecuteAsync(sql, tour);
    }

    public async Task DeleteTourAsync(int id)
    {
        using IDbConnection conn = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM dbo.Tour WHERE Id = @Id";
        await conn.ExecuteAsync(sql, new { Id = id });
    }
}
