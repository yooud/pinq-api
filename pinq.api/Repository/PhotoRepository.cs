using System.Data;
using Dapper;
using pinq.api.Models.Entities;

namespace pinq.api.Repository;

public class PhotoRepository(IDbConnection connection) : IPhotoRepository
{
    public async Task<bool> IsPhotoCanBeAccessed(string uid, string photoCode)
    {
        const string sql = """
                           SELECT 1 
                           FROM photos p
                           JOIN users u ON p.user_id = u.id
                           WHERE u.uid = @uid AND p.image_code = @photoCode
                           """;
        var result = await connection.QueryAsync(sql, new { uid, photoCode });
        return result.SingleOrDefault() != null;
    }

    public async Task<Photo?> GetPhotoByCode(string code)
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
                           WHERE image_code = @code
                           """;
        var photo = await connection.QuerySingleOrDefaultAsync<Photo>(sql, new { code });
        return photo;
    }

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

    public async Task<Photo> CreatePhotoAsync(Photo photo)
    {
        const string sql = """
                           INSERT INTO photos (user_id, photo_type, image_code, image_url, created_at)
                           VALUES (@UserId, @PhotoType::photo_type, @ImageCode, @ImageUrl, CURRENT_TIMESTAMP)
                           RETURNING
                               id as Id,
                               user_id as UserId,
                               photo_type as PhotoType,
                               image_code as ImageCode,
                               image_url as ImageUrl,
                               created_at as CreatedAt
                           """;
        var newPhoto = await connection.QueryFirstAsync<Photo>(sql, photo);
        return newPhoto;
    }
}