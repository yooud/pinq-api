using pinq.api.Models.Dto.Chat;
using pinq.api.Models.Entities;

namespace pinq.api.Repository;

public interface IChatRepository
{
    public Task<ICollection<ChatDto>> GetChatsByUserIdAsync(int userId, int count, int skip);
    
    public Task<int> CountChatsByUserIdAsync(int userId);
    
    public Task<Chat?> GetChatByUsernamesAsync(string username1, string username2);
    
    public Task<ICollection<MessageDto>> GetChatMessagesAsync(int chatId, int count, int skip);
    
    public Task<int> CountChatMessagesAsync(int chatId);
}