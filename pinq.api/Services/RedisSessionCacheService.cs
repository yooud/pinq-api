using StackExchange.Redis;

namespace pinq.api.Services;

public class RedisSessionCacheService(IDatabase cache, ISessionDatabaseService database) : ISessionCacheService
{
    public async Task SetSessionAsync(string uid, string sessionId) => await cache.StringSetAsync(uid, sessionId);

    public async Task<bool> ValidateSessionAsync(string uid, string sessionId)
    {
        var cachedSessionId = await cache.StringGetAsync(uid);
        if (cachedSessionId.HasValue) return cachedSessionId == sessionId;
        var dbSessionId = await database.GetSessionIdAsync(uid);
        if (dbSessionId is null) return false;
        await cache.StringSetAsync(uid, dbSessionId);
        return dbSessionId == sessionId;
    }

    public async Task InvalidateSessionAsync(string uid) => await cache.KeyDeleteAsync(uid);
}
