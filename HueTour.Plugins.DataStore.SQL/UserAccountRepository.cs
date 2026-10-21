using System.Data;
using Dapper;
using HueTour.CoreBusiness;
using HueTour.UseCases.PluginInterfaces.DataStore;

namespace HueTour.Plugins.DataStore.SQL;

public class UserAccountRepository : IUserAccountRepository
{
    private readonly ISqlDbConnectionFactory _connectionFactory;

    public UserAccountRepository(ISqlDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<UserAccount?> GetByUsernameAsync(string username)
    {
        using IDbConnection conn = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM dbo.UserAccount WHERE Username = @Username";
        return await conn.QuerySingleOrDefaultAsync<UserAccount>(sql, new { Username = username });
    }

    public async Task<UserAccount?> ValidateCredentialsAsync(string username, string password)
    {
        using IDbConnection conn = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM dbo.UserAccount WHERE Username = @Username";
        var user = await conn.QuerySingleOrDefaultAsync<UserAccount>(sql, new { Username = username });
        if (user is null) return null;

        return UserAccountPasswordHasher.Verify(user.PasswordHash, password) ? user : null;
    }

    public async Task<bool> RegisterUserAccountAsync(UserAccount user)
    {
        using IDbConnection conn = _connectionFactory.CreateConnection();
        const string checkSql = "SELECT COUNT(1) FROM dbo.UserAccount WHERE Username = @Username";
        int exists = await conn.ExecuteScalarAsync<int>(checkSql, new { Username = user.Username });
        if (exists > 0) return false;

        const string insertSql = @"
            INSERT INTO dbo.UserAccount (Username, PasswordHash, FullName, Role, Email, PhoneNumber)
            VALUES (@Username, @PasswordHash, @FullName, @Role, @Email, @PhoneNumber)";

        int rows = await conn.ExecuteAsync(insertSql, new
        {
            user.Username,
            user.PasswordHash,
            user.FullName,
            Role = string.IsNullOrWhiteSpace(user.Role) ? "Customer" : user.Role,
            user.Email,
            user.PhoneNumber
        });

        return rows > 0;
    }
}
