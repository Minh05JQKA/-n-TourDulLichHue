using System.Data;
using Microsoft.Data.SqlClient;

namespace HueTour.Plugins.DataStore.SQL;

public interface ISqlDbConnectionFactory
{
    IDbConnection CreateConnection();
}

public class SqlDbConnectionFactory : ISqlDbConnectionFactory
{
    private readonly string _connectionString;

    public SqlDbConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
