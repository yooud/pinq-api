using pinq.api.Models.Dto.Profile;

namespace pinq.api.Repository;

public interface IMapRepository
{
    public Task<IEnumerable<ProfileDto>> GetFriendsLocationsAsync(int userId);
    
    public Task<ProfileDto?> GetLocationsAsync(int userId);
}