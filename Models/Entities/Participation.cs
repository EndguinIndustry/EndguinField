namespace EndguinField.Models.Entities;

public class Participation
{
    public int UserId { get; set; }
    public int EventId { get; set; }
    public User User { get; set; } = null!;
    public DateTime JoinedTime { get; set; }
    public string Status { get; set; } = "pending"; // pending / accepted
}
