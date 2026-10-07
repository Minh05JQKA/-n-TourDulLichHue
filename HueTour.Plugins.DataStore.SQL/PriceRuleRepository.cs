using System.Data;
using Dapper;
using HueTour.CoreBusiness;
using HueTour.UseCases.PluginInterfaces.DataStore;

namespace HueTour.Plugins.DataStore.SQL;

public class PriceRuleRepository : IPriceRuleRepository
{
    private readonly ISqlDbConnectionFactory _connectionFactory;

    public PriceRuleRepository(ISqlDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<PriceRule>> GetAllPriceRulesAsync()
    {
        using IDbConnection conn = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM dbo.PriceRule WHERE IsActive = 1 ORDER BY MinAge DESC";
        return await conn.QueryAsync<PriceRule>(sql);
    }

    public async Task<PriceRule?> GetPriceRuleByTypeAsync(string passengerType)
    {
        using IDbConnection conn = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM dbo.PriceRule WHERE PassengerType = @PassengerType AND IsActive = 1";
        return await conn.QuerySingleOrDefaultAsync<PriceRule>(sql, new { PassengerType = passengerType });
    }

    public async Task UpdatePriceRuleAsync(PriceRule priceRule)
    {
        using IDbConnection conn = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE dbo.PriceRule
            SET DisplayName = @DisplayName, RateMultiplier = @RateMultiplier,
                MinAge = @MinAge, MaxAge = @MaxAge, Description = @Description, IsActive = @IsActive
            WHERE Id = @Id";

        await conn.ExecuteAsync(sql, priceRule);
    }
}
