using pinq.api.Models.Entities;

namespace pinq.api.Repository;

public interface IPhotoRepository
{
    public Task<bool> IsPhotoCanBeAccessed(string uid, string photoCode);

    public Task<Photo?> GetPhotoByCode(string code);
    
    public Task<Photo?> GetPhotoByIdAsync(int id);
    
    public Task<Photo> CreatePhotoAsync(Photo photo);
}