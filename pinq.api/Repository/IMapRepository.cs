using pinq.api.Models.Dto.Profile;

namespace pinq.api.Repository;

public interface IMapRepository
{
    public Task<IEnumerable<ProfileDto>> GetFriendsLocationsAsync(int uid);
}