using System.Data;
using Dapper;
using pinq.api.Models.Entities;

namespace pinq.api.Repository;

public class UserRepository(IDbConnection connection) : IUserRepository
{
    public async Task<bool> IsExistsByUid(string uid)
    {
        const string sql = "SELECT 1 FROM users WHERE uid = @uid";
        var result = await connection.QueryAsync(sql, new { uid });
        return result.SingleOrDefault() != null;
    }

    public async Task<bool> IsExistsByEmail(string email)
    {
        const string sql = "SELECT 1 FROM users WHERE email = @email";
        var result = await connection.QueryAsync(sql, new { email });
        return result.SingleOrDefault() != null;
    }

    public async Task CreateUser(string uid, string email)
    {
        const string sql = "INSERT INTO users (uid, email) VALUES (@uid, @email)";
        await connection.ExecuteAsync(sql, new { uid, email });
    }

    public async Task<User> GetUserByUid(string uid)
    {
        const string sql = """
                           SELECT 
                               u.id AS Id,
                               u.uid AS Uid,
                               u.email AS Email,
                               u.is_banned AS IsBanned,
                               u.banned_at AS BannedAt,
                               u.created_at AS CreatedAt
                           FROM users u 
                           WHERE uid = @uid
                           """;
        var user = await connection.QuerySingleAsync<User>(sql, new { uid });
        return user;
    }
}