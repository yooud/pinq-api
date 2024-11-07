using System.Data;
using Dapper;

namespace pinq.api.Repository;

public class UserSessionRepository(IDbConnection connection) : IUserSessionRepository
{
    public async Task UpdateSession(string uid, string fcmToken, Guid sessionId)
    {
        const string sql = """
                           INSERT INTO user_sessions (uid, session_token, fcm_token, last_login) 
                           VALUES (@uid, @sessionId, @fcmToken, CURRENT_TIMESTAMP)
                           ON CONFLICT (uid) DO UPDATE
                           SET session_token = @sessionId,
                               fcm_token = @fcmToken,
                               last_login = CURRENT_TIMESTAMP;
                           """;
        await connection.ExecuteAsync(sql, new { uid, sessionId, fcmToken });
    }

    public async Task DeleteSession(string uid)
    {
        const string sql = "DELETE FROM user_sessions WHERE uid = @uid";
        await connection.ExecuteAsync(sql, new { uid });
    }
}