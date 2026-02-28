using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using EndguinField.Models.ViewModels;

namespace EndguinField.Models;

public class Comment
{
	[BsonId]
	[BsonRepresentation(BsonType.String)]
	public string Id { get; set; } = string.Empty;

	public string Content { get; set; } = string.Empty;

	public DateTime TimeStamp { get; set; }

	public string SenderId { get; set; } = string.Empty;

	public string? RepliedMessageId { get; set; }

	public CommentViewModel ToViewModel(UserSummaryViewModel sender)
	{
		return new CommentViewModel
		{
			Id = Id,
			Content = Content,
			TimeStamp = TimeStamp,
			Sender = sender,
			RepliedMessageId = RepliedMessageId
		};
	}
}
