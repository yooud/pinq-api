using System.Data;
using Dapper;

namespace pinq.api.Repository;

public class UserRepository(IDbConnection connection) : IUserRepository
{
    public async Task<bool> IsExists(string uid)
    {
        const string sql = "SELECT 1 FROM users WHERE uid = @uid";
        var result = await connection.QueryAsync(sql, new { uid });
        return result.SingleOrDefault() != null;
    }

    public async Task CreateUser(string uid, string email)
    {
        const string sql = "INSERT INTO users (uid, email) VALUES (@uid, @email)";
        await connection.ExecuteAsync(sql, new { uid, email });
    }
}