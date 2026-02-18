namespace EndguinField.Models;

public class Comment
{
	public string Id { get; set; } = string.Empty;
	public string SenderId { get; set; } = string.Empty;
	public User Sender { get; set; } = null!;
	public string? RepliedMessageId { get; set; }
	public DateTime TimeStamp { get; set; }
	public string Content { get; set; } = string.Empty;
}
