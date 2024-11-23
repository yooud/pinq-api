using pinq.api.Models.Entities;

namespace pinq.api.Repository;

public interface IFriendRepository
{
    public Task<bool> IsFriendsAsync(int userId1, int userId2);
    
    public Task<Friend> CreateFriendshipAsync(int userId1, int userId2);
}