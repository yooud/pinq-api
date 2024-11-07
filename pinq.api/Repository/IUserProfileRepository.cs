namespace pinq.api.Repository;

public interface IUserProfileRepository
{
    public Task<bool> IsExists(string uid);
}