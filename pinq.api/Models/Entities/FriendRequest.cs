namespace pinq.api.Models.Entities;

public class FriendRequest
{
    public int Id { get; set; }

    public int SenderId { get; set; }

    public int ReceiverId { get; set; }

    public string Status
    {
        get => _status.ToString().ToLower();
        set => _status = (RequestStatus)Enum.Parse(typeof(RequestStatus), string.Concat(value[0].ToString().ToUpper(), value.AsSpan(1)));
    }

    private RequestStatus _status { get; set; }
    
    public DateTime CreatedAt { get; set; }

    public DateTime UploadedAt { get; set; }

    public enum RequestStatus
    {
        Pending,
        Accepted,
        Rejected,
        Canceled
    }
}