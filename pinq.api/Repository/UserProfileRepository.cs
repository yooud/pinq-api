using System.Data;
using Dapper;

namespace pinq.api.Repository;

public class UserProfileRepository(IDbConnection connection) : IUserProfileRepository
{
    public async Task<bool> IsExists(string uid)
    {
        const string sql = "SELECT 1 FROM user_profiles p JOIN users u ON p.user_id = u.id WHERE u.uid = @uid";
        var result = await connection.QueryAsync(sql, new { uid });
        return result.SingleOrDefault() != null;
    }
}