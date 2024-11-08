using System.Data;
using System.Text;
using Dapper;
using pinq.api.Models.Entities;

namespace pinq.api.Repository;

public class UserProfileRepository(IDbConnection connection) : IUserProfileRepository
{
    public async Task<bool> IsExists(string uid)
    {
        const string sql = "SELECT 1 FROM user_profiles p JOIN users u ON p.user_id = u.id WHERE u.uid = @uid";
        var result = await connection.QueryAsync(sql, new { uid });
        return result.SingleOrDefault() != null;
    }

    public async Task<bool> IsUsernameTaken(string username)
    {
        const string sql = "SELECT 1 FROM user_profiles WHERE username = @username";
        var result = await connection.QueryAsync(sql, new { username });
        return result.SingleOrDefault() != null;
    }

    public async Task<Profile> UpdateProfileAsync(string uid, string? username, string? displayName)
    {
        const string sql = """
                            UPDATE user_profiles p
                            SET 
                                username = COALESCE(@username, p.username),
                                display_name = COALESCE(@displayName, p.display_name)
                            FROM users u
                            WHERE p.user_id = u.id AND u.uid = @uid;
                            
                            SELECT 
                                p.user_id AS UserId,
                                p.username AS Username,
                                p.display_name AS DisplayName,
                                p.status AS STATUS,
                                p.photo_id AS PhotoId,
                                p.battery_status AS BatteryStatus
                            FROM user_profiles p 
                            JOIN users u ON p.user_id = u.id 
                            WHERE u.uid = @uid
                            """;
        var result = await connection.QueryFirstAsync<Profile>(sql, new 
        {
            uid,
            username, 
            displayName
        });
        return result;
    }

    public async Task<Profile> CreateProfileAsync(string uid, string username, string displayName)
    {
        const string sql = """
                           INSERT INTO user_profiles (user_id, username, display_name)
                           SELECT u.id, @username, @displayName
                           FROM users u WHERE u.uid = @uid;

                           SELECT 
                               p.user_id AS UserId,
                               p.username AS Username,
                               p.display_name AS DisplayName,
                               p.status AS STATUS,
                               p.photo_id AS PhotoId,
                               p.battery_status AS BatteryStatus
                           FROM user_profiles p 
                           JOIN users u ON p.user_id = u.id 
                           WHERE u.uid = @uid
                           """;
        var result = await connection.QueryFirstAsync<Profile>(sql, new 
        {
            uid,
            username, 
            displayName
        });
        return result;
    }

    public async Task<Profile?> GetProfileByUsername(string username)
    {
        const string sql = """
                           SELECT 
                               p.user_id AS UserId,
                               p.username AS Username,
                               p.display_name AS DisplayName,
                               p.status AS STATUS,
                               p.photo_id AS PhotoId,
                               p.battery_status AS BatteryStatus
                           FROM user_profiles p 
                           WHERE p.username = @username
                           """;
        var result = await connection.QueryFirstOrDefaultAsync<Profile>(sql, new { username });
        return result;
    }
    
    public async Task<Profile?> GetProfileByUid(string uid)
    {
        const string sql = """
                           SELECT 
                               p.user_id AS UserId,
                               p.username AS Username,
                               p.display_name AS DisplayName,
                               p.status AS STATUS,
                               p.photo_id AS PhotoId,
                               p.battery_status AS BatteryStatus
                           FROM user_profiles p 
                           JOIN users u ON p.user_id = u.id 
                           WHERE u.uid = @uid
                           """;
        var result = await connection.QueryFirstOrDefaultAsync<Profile>(sql, new { uid });
        return result;
    }
}