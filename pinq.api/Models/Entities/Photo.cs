namespace pinq.api.Models.Entities;

public class Photo
{
    private string _photoType1;
    public int Id { get; set; }

    public int UserId { get; set; }

    public string PhotoType
    {
        get => _photoType.ToString().ToLower();
        set => _photoType = (Type)Enum.Parse(typeof(Type), string.Concat(value[0].ToString().ToUpper(), value.AsSpan(1)));
    }

    private Type _photoType { get; set; }
    
    public string ImageCode { get; set; }

    public string ImageUrl { get; set; }
    
    public DateTime CreatedAt { get; set; }

    public enum Type
    {
        Avatar,
        Post,
        Chat
    }
}