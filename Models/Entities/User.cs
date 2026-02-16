namespace EndguinField.Models.Entities;

public class User
{
	public int Id { get; set; }
	public string Email { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public string Role { get; set; } = string.Empty;
	public string Bio { get; set; } = string.Empty;
	public string Image { get; set; } = string.Empty;
	public string Contact { get; set; } = string.Empty;
	public DateTime JoinWhen { get; set; }
	public List<string> Friends { get; set; } = new();
	public List<string> BlockLists { get; set; } = new();
	public List<Participation> Participations { get; set; } = new();
	public DateTime BanUntil { get; set; }
	public List<string> FriendRequests { get; set; } = new();
}
