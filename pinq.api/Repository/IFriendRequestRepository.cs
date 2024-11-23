using pinq.api.Models.Entities;

namespace pinq.api.Repository;

public interface IFriendRequestRepository
{
    public Task<FriendRequest?> GetFriendRequestAsync(int userId1, int userId2);
    
    public Task<FriendRequest> CreateFriendRequestAsync(FriendRequest friendRequest);
    
    public Task<FriendRequest?> AcceptFriendRequestAsync(int requestId);
}