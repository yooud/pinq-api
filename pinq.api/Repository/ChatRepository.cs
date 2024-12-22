using System.Data;
using System.Text.Json;
using Dapper;
using pinq.api.Models.Dto.Chat;
using pinq.api.Models.Entities;

namespace pinq.api.Repository;

public class ChatRepository(IDbConnection connection) : IChatRepository
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
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
                chat.LastMessage.Content = JsonSerializer.Deserialize<MessageContent>(message.Content.ToString(),_options);
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
}