namespace pinq.api.Repository;

public interface IUserSessionRepository
{
    public Task UpdateSession(string uid, string fcmToken, Guid sessionId);
    
    public Task DeleteSession(string uid);
}