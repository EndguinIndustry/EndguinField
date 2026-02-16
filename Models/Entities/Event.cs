namespace EndguinField.Models.Entities;

public class Event
{
    public int EventId { get; set; }
    public int OwnerId { get; set; }
    public User Owner { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Images { get; set; } = new();
    public string BackgroundImage { get; set; } = string.Empty;
    public List<Participation> Participations { get; set; } = new();
    public string Location { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int MaxEnrolls { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = "open"; // open / close / cancelled
    public List<string> BlackLists { get; set; } = new();
    public List<Comment> Comments { get; set; } = new();
}
