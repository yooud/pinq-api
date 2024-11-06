namespace pinq.api.Services;

public interface ISessionCacheService
{
    public Task SetSessionAsync(string uid, string sessionId);
    
    public Task<bool> ValidateSessionAsync(string uid, string sessionId);
    
    public Task InvalidateSessionAsync(string uid);
}