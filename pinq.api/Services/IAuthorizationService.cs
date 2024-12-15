using FirebaseAdmin.Auth;

namespace pinq.api.Services;

public interface IAuthorizationService
{
    public Task<bool> ValidateTokenAsync(string token);
    
    public Task<FirebaseToken> GetTokenAsync(string token);
    
    public Task<string> GetUserRoleAsync(string uid);
    
    public Task<bool> SetUserRoleAsync(string uid, string role);
}