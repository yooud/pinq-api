using Dapper;
using Npgsql;

namespace pinq.api.Services;

public class SessionDatabaseService(IConfiguration configuration) : ISessionDatabaseService
{
    public async Task<string?> GetSessionIdAsync(string uid)
    {
        await using var connection = new NpgsqlConnection(configuration.GetConnectionString("DefaultConnection"));
        const string query = "SELECT session_token::text FROM user_sessions WHERE uid = @uid";
        var result = await connection.ExecuteScalarAsync<string>(query, new { uid });
        return result;
    }
}