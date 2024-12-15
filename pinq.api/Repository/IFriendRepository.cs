using pinq.api.Models.Dto.Profile;
using pinq.api.Models.Entities;

namespace pinq.api.Repository;

public interface IFriendRepository
{
    public Task<bool> IsFriendsAsync(int userId1, int userId2);
    
    public Task<Friend> CreateFriendshipAsync(int userId1, int userId2);
    
    public Task DeleteFriendshipAsync(int userId1, int userId2);
    
    public Task<IEnumerable<Profile>> GetFriendsAsync(int userId, int count, int skip);

    public Task<int> CountFriendsAsync(int userId);
    
    public Task<IEnumerable<int>> GetFriendIdsAsync(int userId);
}