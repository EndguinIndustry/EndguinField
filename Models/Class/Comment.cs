using EndguinField.Models.ViewModels;
namespace EndguinField.Models;

public class Comment
{
	public string Id { get; set; } = string.Empty;
	public string SenderId { get; set; } = string.Empty;
	public string? RepliedMessageId { get; set; }
	public DateTime TimeStamp { get; set; }
	public string Content { get; set; } = string.Empty;

	public CommentViewModel ToViewModel(UserSummaryViewModel sender)
	{
		return new CommentViewModel
		{
			Id = Id,
			Content = Content,
			TimeStamp = TimeStamp,
			RepliedMessageId = RepliedMessageId,
			Sender = sender ?? new UserSummaryViewModel { Id = SenderId, Name = "Unknown" }
		};
	}
}
