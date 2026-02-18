namespace EndguinField.Models;

public class Participation
{
	public string Id { get; set; }
	public string UserId { get; set; }
	public string EventId { get; set; }
	public string User { get; set; } = null!;
	public DateTime JoinedTime { get; set; }
	public string Status { get; set; } = "pending"; // pending / accepted
}
