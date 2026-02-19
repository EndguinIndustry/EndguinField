namespace EndguinField.Models.ViewModels;

public class EventViewModel
{
	public string Id { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public List<string> Images { get; set; } = new();
	public string BackgroundImage { get; set; } = string.Empty;
	public string Location { get; set; } = string.Empty;
	public DateTime StartTime { get; set; }
	public DateTime EndTime { get; set; }
	public int MaxParticipants { get; set; }
	public string Category { get; set; } = string.Empty;
	public string Status { get; set; } = "Open";

	public UserSummaryViewModel Owner { get; set; } = null!;
	public List<ParticipationViewModel> Participants { get; set; } = new();
	public List<CommentViewModel> Comments { get; set; } = new();

	public bool IsOwner { get; set; }
	public bool IsJoined { get; set; }
	public bool IsCreateMode { get; set; }
	public string? UserStatus { get; set; }

	public int ParticipationCount => Participants?.Count ?? 0;
}

public class UserSummaryViewModel
{
	public string Id { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public string Image { get; set; } = string.Empty;
}

public class ParticipationViewModel
{
	public string UserId { get; set; } = string.Empty;
	public string UserName { get; set; } = string.Empty;
	public string UserImage { get; set; } = string.Empty;
	public string Status { get; set; } = string.Empty;
	public DateTime JoinedTime { get; set; }
}

public class CommentViewModel
{
	public string Id { get; set; } = string.Empty;
	public string Content { get; set; } = string.Empty;
	public DateTime TimeStamp { get; set; }
	public UserSummaryViewModel Sender { get; set; } = null!;
	public string? RepliedMessageId { get; set; }
}
