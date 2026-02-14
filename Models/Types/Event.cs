namespace EndguinField.Models;

public class Event
{
	public int EventId { get; set; }
	public int OwnerId { get; set; }
	public string Name { get; set; }
	public string Description { get; set; }
	public List<string> Images { get; set; }
	public string BackgroundImage { get; set; }
	public List<string> Enrolls { get; set; }
	public string Location { get; set; }
	public DateTime StartTime { get; set; }
	public DateTime EndTime { get; set; }
	public int MaxEnrolls { get; set; }
	public string Category { get; set; }
	public string Status { get; set; } // open / close / cancelled
	public List<string> BlackLists { get; set; }
	public List<Comment> Comments { get; set; }
}