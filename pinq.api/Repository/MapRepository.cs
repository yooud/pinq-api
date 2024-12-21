using System.Data;
using Dapper;
using pinq.api.Models.Dto.Map;

namespace pinq.api.Repository;

public class MapRepository(IDbConnection connection) : IMapRepository
{
    public async Task<IEnumerable<UserDto>> GetFriendsLocationsAsync(int userId)
    {
        const string sql = """
                           SELECT
                               p.user_id AS Id,
                               p.username AS Username,
                               p.display_name AS DisplayName,
                               ph.image_url AS ProfilePictureUrl,
                               p.status AS Status,
                               EXTRACT(EPOCH FROM l.created_at) AS LastActivity,
                               ST_X(l.geom) AS Lng,
                               ST_Y(l.geom) AS Lat
                           FROM friends f
                           JOIN user_profiles p 
                               ON (f.user_id = @userId AND p.user_id = f.friend_id)
                               OR (f.friend_id = @userId AND p.user_id = f.user_id)
                           LEFT JOIN (
                               SELECT DISTINCT ON (l.user_id) 
                                      l.user_id, l.geom, l.created_at
                               FROM locations l
                               ORDER BY l.user_id, l.created_at DESC
                           ) l ON l.user_id = p.user_id
                           LEFT JOIN photos ph ON p.photo_id = ph.id
                           WHERE f.user_id = @userId OR f.friend_id = @userId;
                           """;
        
        var profiles = await connection.QueryAsync<UserDto, LocationDto, UserDto>(
            sql,
            (profile, location) =>
            {
                profile.Location = location;
                return profile;
            },
            new { userId },
            splitOn: "Lng"
        );
        
        return profiles;
    }

    public async Task<UserDto?> GetLocationsAsync(int userId)
    {
        const string sql = """
                           SELECT 
                               p.user_id AS Id,
                               p.username AS Username,
                               p.display_name AS DisplayName,
                               ph.image_url AS ProfilePictureUrl,
                               p.status AS Status,
                               EXTRACT(EPOCH FROM l.created_at) AS LastActivity,
                               ST_X(l.geom) AS Lng,
                               ST_Y(l.geom) AS Lat
                           FROM user_profiles p
                           LEFT JOIN (
                               SELECT DISTINCT ON (user_id) 
                                      user_id, geom, created_at
                               FROM locations
                               ORDER BY user_id, created_at DESC
                           ) l ON l.user_id = p.user_id
                           LEFT JOIN photos ph ON p.photo_id = ph.id
                           WHERE p.user_id = @userId;
                           """;
        
        var results = await connection.QueryAsync<UserDto, LocationDto, UserDto>(
            sql,
            (profile, location) =>
            {
                profile.Location = location;
                return profile;
            },
            new { userId },
            splitOn: "Lng"
        );

        return results.FirstOrDefault();
    }

    public async Task UpdateLocationAsync(int userId, LocationDto location)
    {
        const string sql = """
                           INSERT INTO locations (user_id, geom)
                           VALUES (@userId, ST_SetSRID(ST_MakePoint(@lng, @lat), 4326));
                           """;
        
        await connection.ExecuteAsync(sql, new { userId, location.Lng, location.Lat });
    }
}