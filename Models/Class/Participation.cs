using EndguinField.Models.ViewModels;
namespace EndguinField.Models;

public class Participation
{
	public string Id { get; set; } = string.Empty;
	public string UserId { get; set; } = string.Empty;
	public string EventId { get; set; } = string.Empty;
	public string User { get; set; } = null!;
	public DateTime JoinedTime { get; set; }
	public string Status { get; set; } = "Pending"; // Pending / Accepted / Rejected

	public ParticipationViewModel ToViewModel(UserSummaryViewModel userSummary)
	{
		return new ParticipationViewModel
		{
			UserId = UserId,
			UserName = userSummary?.Name ?? string.Empty,
			UserImage = userSummary?.Image ?? string.Empty,
			Status = Status,
			JoinedTime = JoinedTime
		};
	}
}
