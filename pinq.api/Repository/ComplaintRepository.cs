using System.Data;
using Dapper;
using pinq.api.Models.Entities;

namespace pinq.api.Repository;

public class ComplaintRepository(IDbConnection connection) : IComplaintRepository
{
    public async Task<Complaint> CreateComplaintAsync(Complaint complaint)
    {
        const string sql = """
                           INSERT INTO complaints (user_id, target_user_id, content_type, content_id, reason, status)
                           VALUES (@UserId, @TargetUserID, @ContentType::complaint_content_type, @ContentId, @Reason, 'pending')
                           RETURNING
                               id as Id,
                               user_id as UserId,
                               target_user_id AS TargetUserID, 
                               content_type AS ContentType, 
                               content_id AS ContentId, 
                               reason AS Reason, 
                               status AS Status,
                               created_at as CreatedAt
                           """;
        var newComplaint = await connection.QueryFirstAsync<Complaint>(sql, complaint);
        return newComplaint;
    }
}