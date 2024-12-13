using FirebaseAdmin.Auth;

namespace pinq.api.Services;

public class FirebaseAuthorizationService : IAuthorizationService
{
    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var result = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token);
            return result != null;
        }
        catch (FirebaseAuthException ex)
        {
            return false;
        }
    }

    public async Task<FirebaseToken> GetTokenAsync(string token) => await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token);
}