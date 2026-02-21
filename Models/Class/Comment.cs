namespace EndguinField.Models;

public class Comment
{
	public string Id { get; set; } = string.Empty;
	public string Content { get; set; } = string.Empty;
	public DateTime TimeStamp { get; set; }
	public string SenderId { get; set; } = string.Empty;
	public string? RepliedCommentId { get; set; }
}
