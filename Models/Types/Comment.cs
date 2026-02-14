namespace EndguinField.Models;

public class Comment
{
	public string? Id { get; set; }
	public int SenderId { get; set; }
	public string? RepliedMessageId { get; set; }
	public DateTime TimeStamp { get; set; }
	public string? Content { get; set; }
}