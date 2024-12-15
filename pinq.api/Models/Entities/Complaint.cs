namespace pinq.api.Models.Entities;

public class Complaint
{
    private ComplaintStatus _status { get; set; }
    private ComplaintContentType _contentType { get; set; }
    
    public int Id { get; set; }

    public int UserId { get; set; }
    
    public int TargetUserId { get; set; }

    public string ContentType
    {
        get => _contentType.ToString().ToLower();
        set => _contentType = (ComplaintContentType)Enum.Parse(typeof(ComplaintContentType), string.Concat(value[0].ToString().ToUpper(), value.AsSpan(1)));
    }

    public int? ContentId { get; set; }

    public string Reason { get; set; }

    public string Status
    {
        get => _status.ToString().ToLower();
        set => _status = (ComplaintStatus)Enum.Parse(typeof(ComplaintStatus), string.Concat(value[0].ToString().ToUpper(), value.AsSpan(1)));
    }

    public DateTime CreatedAt { get; set; }
    
    public enum ComplaintContentType
    {
        User,
        Avatar,
        Post
    }
    
    public enum ComplaintStatus
    {
        Pending,
        InReview,
        Approved,
        Denied
    }
}