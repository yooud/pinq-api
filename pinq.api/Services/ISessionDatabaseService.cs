namespace pinq.api.Services;

public interface ISessionDatabaseService
{
    public Task<string?> GetSessionIdAsync(string uid);
}