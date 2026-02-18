namespace EndguinField.Models;

public class Participation

{
	public string UserId { get; set; } = string.Empty;
	public string EventId { get; set; } = string.Empty;
	public User User { get; set; } = null!;
	public DateTime JoinedTime { get; set; }
	public string Status { get; set; } = "pending"; // pending / accepted
}
