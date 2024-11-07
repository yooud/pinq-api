using StackExchange.Redis;

namespace pinq.api.Services;

public class RedisSessionCacheService : ISessionCacheService
{
    private readonly IDatabase _cache;
    private readonly ISessionDatabaseService _database;
    
    public RedisSessionCacheService(ConnectionMultiplexer redis, ISessionDatabaseService database)
    {
        _cache = redis.GetDatabase();
        _database = database;
    }
    
    public async Task SetSessionAsync(string uid, string sessionId) => await _cache.StringSetAsync(uid, sessionId);

    public async Task<bool> ValidateSessionAsync(string uid, string sessionId)
    {
        var cachedSessionId = await _cache.StringGetAsync(uid);
        if (cachedSessionId.HasValue) return cachedSessionId == sessionId;
        var dbSessionId = await _database.GetSessionIdAsync(uid);
        if (dbSessionId is null) return false;
        await _cache.StringSetAsync(uid, dbSessionId);
        return dbSessionId == sessionId;
    }

    public async Task InvalidateSessionAsync(string uid) => await _cache.KeyDeleteAsync(uid);
}
