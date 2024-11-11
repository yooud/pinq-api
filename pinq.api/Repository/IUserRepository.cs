using pinq.api.Models.Entities;

namespace pinq.api.Repository;

public interface IUserRepository
{
    public Task<bool> IsExists(string uid);
    
    public Task CreateUser(string uid, string email);
    
    public Task<User> GetUserByUid(string uid);
}