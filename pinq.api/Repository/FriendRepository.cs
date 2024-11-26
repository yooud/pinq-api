using System.Data;
using Dapper;
using pinq.api.Models.Entities;

namespace pinq.api.Repository;

public class FriendRepository(IDbConnection connection) : IFriendRequestRepository, IFriendRepository
{
    public async Task<FriendRequest?> GetFriendRequestAsync(int userId1, int userId2)
    {
        const string sql = """
                           SELECT
                               fr.id AS Id,
                               fr.sender_id AS SenderId,
                               fr.receiver_id AS ReceiverId,
                               fr.status AS Status,
                               fr.created_at AS CreatedAt,
                               fr.updated_at AS UpdatedAt
                           FROM friends_requests fr
                           WHERE ((fr.sender_id = @userId1 AND fr.receiver_id = @userId2) OR
                                 (fr.sender_id = @userId2 AND fr.receiver_id = @userId1)) AND 
                                 fr.status = 'pending'
                           ORDER BY fr.updated_at DESC
                           LIMIT 1
                           """;
        var newFriendRequest = await connection.QueryFirstOrDefaultAsync<FriendRequest>(sql, new { userId1, userId2 });
        return newFriendRequest;
    }

    public async Task<FriendRequest> CreateFriendRequestAsync(FriendRequest friendRequest)
    {
        const string sql = """
                           INSERT INTO friends_requests (sender_id, receiver_id, status, created_at, updated_at)
                           VALUES (@SenderId, @ReceiverId, 'pending', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
                           RETURNING
                               id AS Id,
                               sender_id AS SenderId,
                               receiver_id AS ReceiverId,
                               status AS Status,
                               created_at AS CreatedAt,
                               updated_at AS UpdatedAt
                           """;
        var newFriendRequest = await connection.QueryFirstAsync<FriendRequest>(sql, friendRequest);
        return newFriendRequest;
    }

    public async Task<bool> IsFriendsAsync(int userId1, int userId2)
    {
        if (userId1 > userId2)
            return await IsFriendsAsync(userId2, userId1);
        
        const string sql = """
                           SELECT 1
                           FROM friends f
                           WHERE f.user_id = @UserId1 AND f.friend_id = @UserId2
                           """;
        var result = await connection.QueryAsync(sql, new { userId1, userId2 });
        return result.SingleOrDefault() != null;
    }

    public async Task<FriendRequest?> AcceptFriendRequestAsync(int requestId)
    {
        const string sql = """
                           UPDATE friends_requests fr 
                           SET status = 'accepted' AND 
                               updated_at = CURRENT_TIMESTAMP
                           WHERE fr.id = @requestId
                           RETURNING
                               id AS Id,
                               sender_id AS SenderId,
                               receiver_id AS ReceiverId,
                               status AS Status,
                               created_at AS CreatedAt,
                               updated_at AS UpdatedAt
                           """;
        var friendRequest = await connection.QueryFirstOrDefaultAsync<FriendRequest>(sql, new { requestId });
        return friendRequest;
    }

    public async Task<Friend> CreateFriendshipAsync(int userId1, int userId2)
    {
        if (userId1 > userId2)
            return await CreateFriendshipAsync(userId2, userId1);
        
        const string sql = """
                           INSERT INTO friends (user_id, friend_id, created_at) 
                           VALUES (@userId1, @userId2, CURRENT_TIMESTAMP)
                           RETURNING
                               user_id AS UserId,
                               friend_id AS FriendId,
                               created_at AS CreatedAt
                           """;
        var friendship = await connection.QueryFirstAsync<Friend>(sql, new { userId1, userId2 });
        return friendship;
    }
}