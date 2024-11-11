using pinq.api.Models.Entities;

namespace pinq.api.Repository;

public interface IPhotoRepository
{
    public Task<Photo?> GetPhotoByIdAsync(int id);
    
    public Task<Photo> CreatePhotoAsync(Photo photo);
}