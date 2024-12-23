using pinq.api.Models.Dto.Admin;
using pinq.api.Models.Entities;

namespace pinq.api.Repository;

public interface IUserProfileRepository
{
    public Task<bool> IsExists(string uid);
    
    public Task<bool> IsUsernameTaken(string username);
    
    public Task<Profile> UpdateProfileAsync(string uid, string? username, string? displayName, int? photoId);
    
    public Task<Profile> CreateProfileAsync(string uid, string username, string displayName);

    public Task<Profile?> GetProfileByUsername(string username);
    
    public Task<Profile?> GetProfileByUid(string uid);
    
    public Task<IEnumerable<UserDto>> GetProfilesAsync(int count, int skip);
    
    public Task<int> CountProfilesAsync();
}