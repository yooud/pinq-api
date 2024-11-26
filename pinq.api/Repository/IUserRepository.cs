using pinq.api.Models.Entities;

namespace pinq.api.Repository;

public interface IUserRepository
{
    public Task<bool> IsExistsByUid(string uid);
    
    public Task<bool> IsExistsByEmail(string email);
    
    public Task CreateUser(string uid, string email);
    
    public Task<User> GetUserByUid(string uid);
}