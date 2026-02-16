namespace EndguinField.Models.Entities;

public class Comment
{
    public string Id { get; set; } = string.Empty;
    public int SenderId { get; set; }
    public User Sender { get; set; } = null!;
    public string? RepliedMessageId { get; set; }
    public DateTime TimeStamp { get; set; }
    public string Content { get; set; } = string.Empty;
}
