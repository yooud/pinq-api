using System.Data;
using System.Text.Json;
using Dapper;
using pinq.api.Models.Dto.Chat;
using pinq.api.Models.Entities;

namespace pinq.api.Repository;

public class ChatRepository(IDbConnection connection) : IChatRepository
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };
    
    public async Task<ICollection<ChatDto>> GetChatsByUserIdAsync(int userId, int count, int skip)
    {
        const string sql = """
                           SELECT 
                               c.id AS Id,
                               CASE 
                                   WHEN c.user_id1 = @userId THEN p2.username
                                   ELSE p1.username
                               END AS Username,
                               CASE 
                                   WHEN c.user_id1 = @userId THEN p2.display_name
                                   ELSE p1.display_name
                               END AS DisplayName,
                               CASE 
                                   WHEN c.user_id1 = @userId THEN ph2.image_url
                                   ELSE ph1.image_url
                               END AS PictureUrl,
                               m.content AS Content, 
                               EXTRACT(EPOCH FROM m.sent_at) AS SentAt, 
                               EXTRACT(EPOCH FROM m.edited_at) AS EditedAt, 
                               EXTRACT(EPOCH FROM m.seen_at) AS SeenAt,
                               m.sender_username AS SenderUsername
                           FROM chats c
                           JOIN user_profiles p1 ON c.user_id1 = p1.user_id
                           LEFT JOIN photos ph1 ON p1.photo_id = ph1.id
                           JOIN user_profiles p2 ON c.user_id2 = p2.user_id
                           LEFT JOIN photos ph2 ON p2.photo_id = ph2.id
                           LEFT JOIN (
                                 SELECT DISTINCT ON (m.chat_id) 
                                     m.chat_id, 
                                     m.content, 
                                     m.sent_at, 
                                     m.edited_at, 
                                     m.seen_at,
                                     p.username AS sender_username
                                 FROM chat_messages m
                                 JOIN user_profiles p ON m.sender_id = p.user_id
                                 ORDER BY chat_id, sent_at DESC
                           ) m on c.id = m.chat_id
                           WHERE c.user_id1 = @userId OR c.user_id2 = @userId
                           ORDER BY m.sent_at DESC
                           LIMIT @count
                           OFFSET @skip
                           """;
        var result = await connection.QueryAsync<ChatDto, ChatProfileDto, MessageDto, ChatDto>(
            sql,
            (chat, profile, message) =>
            {
                chat.Profile = profile;
                chat.LastMessage = message;
                chat.LastMessage.Content = JsonSerializer.Deserialize<MessageContent>(message.Content.ToString(),Options);
                return chat;
            },
            new { userId, count, skip },
            splitOn: "Username,Content"
        );

        return result.ToList();
    }
    
    public async Task<int> CountChatsByUserIdAsync(int userId)
    {
        const string sql = "SELECT COUNT(*) FROM chats c WHERE c.user_id1 = @userId OR c.user_id2 = @userId";
        var result = await connection.QuerySingleAsync<int>(sql, new { userId });
        return result;
    }
    
    public async Task<Chat?> GetChatByUsernamesAsync(string username1, string username2)
    {
        const string sql = """
                           SELECT * 
                           FROM chats c 
                           JOIN user_profiles p1 ON c.user_id1 = p1.user_id
                           JOIN user_profiles p2 ON c.user_id2 = p2.user_id
                           WHERE (p1.username = @username1 AND p2.username = @username2)
                              OR (p1.username = @username2 AND p2.username = @username1)
                           """;
        var result = await connection.QuerySingleOrDefaultAsync<Chat>(sql, new { username1, username2 });
        return result;
    }
    
    public async Task<ICollection<MessageDto>> GetChatMessagesAsync(int chatId, int count, int skip)
    {
        const string sql = """
                           SELECT 
                               m.id AS Id,
                               m.content AS Content, 
                               EXTRACT(EPOCH FROM m.sent_at) AS SentAt, 
                               EXTRACT(EPOCH FROM m.edited_at) AS EditedAt, 
                               EXTRACT(EPOCH FROM m.seen_at) AS SeenAt,
                               p.username AS SenderUsername
                           FROM chat_messages m
                           JOIN user_profiles p ON m.sender_id = p.user_id
                           WHERE m.chat_id = @chatId
                           ORDER BY m.sent_at DESC
                           LIMIT @count
                           OFFSET @skip
                           """;
        var result = await connection.QueryAsync<MessageDto>(sql, new { chatId, count, skip });
        var messages = result.ToList();
        foreach (var message in messages) 
            message.Content = JsonSerializer.Deserialize<MessageContent>(message.Content.ToString(), Options);
        return messages.ToList();
    }
    
    public async Task<int> CountChatMessagesAsync(int chatId)
    {
        const string sql = "SELECT COUNT(*) FROM chat_messages m WHERE m.chat_id = @chatId";
        var result = await connection.QuerySingleAsync<int>(sql, new { chatId });
        return result;
    }
    
    public async Task<MessageDto> SendMessageAsync(int chatId, int senderId, object messageContent)
    {
        const string sql = """
                           WITH inserted_message AS (
                               INSERT INTO chat_messages (chat_id, sender_id, content)
                               VALUES (@chatId, @senderId, @content::json)
                               RETURNING 
                                   id AS Id, 
                                   content AS Content, 
                                   EXTRACT(EPOCH FROM sent_at) AS SentAt, 
                                   EXTRACT(EPOCH FROM edited_at) AS EditedAt, 
                                   EXTRACT(EPOCH FROM seen_at) AS SeenAt, 
                                   sender_id
                           )
                           SELECT 
                               im.Id, 
                               im.Content, 
                               im.SentAt, 
                               im.EditedAt, 
                               im.SeenAt,
                               up.username AS SenderUsername
                           FROM inserted_message im
                           JOIN user_profiles up ON im.sender_id = up.user_id;
                           """;
        var content = JsonSerializer.Serialize(messageContent, Options);
        var message = await connection.QueryFirstAsync<MessageDto>(sql, new { chatId, senderId, content });
        message.Content = messageContent;
        return message;
    }
    
    public async Task<ICollection<MessageDto>> GetChatMessagesUpdatesAsync(int chatId, long lastUpdate)
    {
        const string sql = """
                           SELECT 
                               m.id AS Id,
                               m.content AS Content, 
                               EXTRACT(EPOCH FROM m.sent_at) AS SentAt, 
                               EXTRACT(EPOCH FROM m.edited_at) AS EditedAt, 
                               EXTRACT(EPOCH FROM m.seen_at) AS SeenAt,
                               p.username AS SenderUsername
                           FROM chat_messages m
                           JOIN user_profiles p ON m.sender_id = p.user_id
                           WHERE m.chat_id = @chatId AND EXTRACT(EPOCH FROM m.sent_at) > @lastUpdate
                           ORDER BY m.updated_at DESC
                           """;
        var result = await connection.QueryAsync<MessageDto>(sql, new { chatId, lastUpdate });
        var messages = result.ToList();
        foreach (var message in messages) 
            message.Content = JsonSerializer.Deserialize<MessageContent>(message.Content.ToString(), Options);
        return messages.ToList();
    }
}