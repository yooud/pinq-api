using pinq.api.Models.Dto.Chat;

namespace pinq.api.Repository;

public interface IChatRepository
{
    public Task<ICollection<ChatDto>> GetChatsByUserIdAsync(int userId, int count, int skip);
    
    public Task<int> CountChatsByUserIdAsync(int userId);
}