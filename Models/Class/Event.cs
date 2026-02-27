namespace EndguinField.Models;

public class Event
{
	public string Id { get; set; } = string.Empty;
	public string OwnerId { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public List<string> Images { get; set; } = new List<string>();
	public string BackgroundImage { get; set; } = string.Empty;
	public List<string> Participants { get; set; } = new List<string>();
	public string Location { get; set; } = string.Empty;
	public DateTime StartTime { get; set; }
	public DateTime EndTime { get; set; }
	public int MaxParticipants { get; set; }
	public string Category { get; set; } = string.Empty;
	public string Status { get; set; } = "Open"; // Open / Close / Cancelled
	public List<string> BlackLists { get; set; } = new List<string>();
	public List<string> Comments { get; set; } = new List<string>();
}
