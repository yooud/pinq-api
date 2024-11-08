using pinq.api.Models.Entities;

namespace pinq.api.Repository;

public interface IUserProfileRepository
{
    public Task<bool> IsExists(string uid);
    
    public Task<bool> IsUsernameTaken(string username);
    
    public Task<Profile> UpdateProfileAsync(string uid, string? username, string? displayName);
    
    public Task<Profile> CreateProfileAsync(string uid, string username, string displayName);
}