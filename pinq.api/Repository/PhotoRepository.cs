using System.Data;
using Dapper;
using pinq.api.Models.Entities;

namespace pinq.api.Repository;

public class PhotoRepository(IDbConnection connection) : IPhotoRepository
{
    public async Task<Photo?> GetPhotoByIdAsync(int id)
    {
        const string sql = """
                           SELECT 
                               id AS Id, 
                               user_id AS UserId, 
                               photo_type AS PhotoType,
                               image_code AS ImageCode,
                               image_url AS ImageUrl,
                               created_at AS CreatedAt
                           FROM photos 
                           WHERE id = @id
                           """;
        var photo = await connection.QuerySingleOrDefaultAsync<Photo>(sql, new { id });
        return photo;
    }
}