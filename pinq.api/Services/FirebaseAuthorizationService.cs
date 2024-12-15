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
    
    public async Task<string> GetUserRoleAsync(string uid)
    {
        var firebaseUser = await FirebaseAuth.DefaultInstance.GetUserAsync(uid);
        var claims = firebaseUser.CustomClaims;
        var role = claims.TryGetValue("role", out var claim) ? claim.ToString() : "user";
        return role ?? "user";
    }

    public async Task<bool> SetUserRoleAsync(string uid, string role)
    {
        try
        {
            var user = await FirebaseAuth.DefaultInstance.GetUserAsync(uid);

            if (user is null)
                return false;
        
            var newClaims = user.CustomClaims.ToDictionary(claim => claim.Key, claim => claim.Value);
            newClaims["role"] = role;
        
            await FirebaseAuth.DefaultInstance.SetCustomUserClaimsAsync(uid, newClaims);
            return true;
        }
        catch (FirebaseAuthException ex)
        {
            Console.WriteLine(ex);
            return false;
        }
    }
}