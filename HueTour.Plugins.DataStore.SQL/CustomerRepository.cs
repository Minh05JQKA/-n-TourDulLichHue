using System.Data;
using Dapper;
using HueTour.CoreBusiness;
using HueTour.UseCases.PluginInterfaces.DataStore;

namespace HueTour.Plugins.DataStore.SQL;

public class CustomerRepository : ICustomerRepository
{
    private readonly ISqlDbConnectionFactory _connectionFactory;

    public CustomerRepository(ISqlDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Customer?> GetCustomerByIdAsync(int id)
    {
        using IDbConnection conn = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM dbo.Customer WHERE Id = @Id";
        return await conn.QuerySingleOrDefaultAsync<Customer>(sql, new { Id = id });
    }

    public async Task<Customer?> GetCustomerByPhoneAsync(string phone)
    {
        using IDbConnection conn = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM dbo.Customer WHERE PhoneNumber = @Phone";
        return await conn.QuerySingleOrDefaultAsync<Customer>(sql, new { Phone = phone });
    }

    public async Task<Customer?> GetCustomerByEmailAsync(string email)
    {
        using IDbConnection conn = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM dbo.Customer WHERE Email = @Email";
        return await conn.QuerySingleOrDefaultAsync<Customer>(sql, new { Email = email });
    }

    public async Task<int> AddCustomerAsync(Customer customer)
    {
        using IDbConnection conn = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO dbo.Customer (FullName, Email, PhoneNumber, Address, IdentityCard, CreatedAt)
            VALUES (@FullName, @Email, @PhoneNumber, @Address, @IdentityCard, @CreatedAt);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        return await conn.ExecuteScalarAsync<int>(sql, customer);
    }

    public async Task UpdateCustomerAsync(Customer customer)
    {
        using IDbConnection conn = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE dbo.Customer
            SET FullName = @FullName, Email = @Email, PhoneNumber = @PhoneNumber,
                Address = @Address, IdentityCard = @IdentityCard
            WHERE Id = @Id";

        await conn.ExecuteAsync(sql, customer);
    }
}
