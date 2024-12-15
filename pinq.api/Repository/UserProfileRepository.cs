using System.Data;
using System.Text;
using Dapper;
using pinq.api.Models.Dto.Admin;
using pinq.api.Models.Dto.Profile;
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

    public async Task<Profile> UpdateProfileAsync(string uid, string? username, string? displayName, int? photoId)
    {
        const string sql = """
                            UPDATE user_profiles p
                            SET 
                                username = COALESCE(@username, p.username),
                                display_name = COALESCE(@displayName, p.display_name),
                                photo_id = COALESCE(@photoId, p.photo_id)
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
            displayName,
            photoId
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

    public async Task<IEnumerable<UserDto>> GetProfilesAsync(int count, int skip)
    {
        const string sql = """
                           SELECT 
                               u.id AS Id,
                               u.uid AS Uid,
                               u.email AS Email,
                               u.is_banned AS IsBanned,
                               EXTRACT(EPOCH FROM u.banned_at) AS BannedAt,
                               EXTRACT(EPOCH FROM u.created_at) AS CreatedAt,
                               p.username AS Username,
                               p.display_name AS DisplayName,
                               ph.image_url AS ProfilePictureUrl
                           FROM user_profiles p
                           JOIN users u ON p.user_id = u.id 
                           LEFT JOIN photos ph ON p.photo_id = ph.id
                           LIMIT :count
                           OFFSET :skip
                           """;
        
        var users = await connection.QueryAsync<UserDto, ProfileDto, UserDto>(
            sql,
            (user, profile) =>
            {
                user.Profile = profile;
                return user;
            },
            new { count, skip },
            splitOn: "Username"
        );
        return users;
    }

    public async Task<int> CountProfilesAsync()
    {
        const string sql = """
                           SELECT COUNT(*)
                           FROM user_profiles p 
                           """;

        var result = await connection.ExecuteScalarAsync(sql);
        return Convert.ToInt32(result);
    }
}