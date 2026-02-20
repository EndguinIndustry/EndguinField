using EndguinField.Models.ViewModels;
namespace EndguinField.Models;

public class Participation
{
	public string Id { get; set; }
	public string UserId { get; set; }
	public string EventId { get; set; }
	public string User { get; set; } = null!;
	public DateTime JoinedTime { get; set; }
	public string Status { get; set; } = "pending"; // pending / accepted

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
