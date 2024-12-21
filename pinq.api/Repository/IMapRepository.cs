using pinq.api.Models.Dto.Map;

namespace pinq.api.Repository;

public interface IMapRepository
{
    public Task<IEnumerable<UserDto>> GetFriendsLocationsAsync(int userId);
    
    public Task<UserDto?> GetLocationsAsync(int userId);
    
    public Task UpdateLocationAsync(int userId, LocationDto location);
}