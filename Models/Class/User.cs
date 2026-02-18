namespace EndguinField.Models
{
	public class User
	{
		public string Id { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public string Role { get; set; } = string.Empty;
		public string? Bio { get; set; }
		public string? Image { get; set; }
		public string? Contact { get; set; }
		public DateTime JoinWhen { get; set; }
		public List<int> Friends { get; set; } = new List<int>();
		public List<User> BlockLists { get; set; } = new List<User>();
		public List<Participation> Enrolls { get; set; } = new List<Participation>();
		public DateTime? BanUntil { get; set; }
		public List<int> FriendRequests { get; set; } = new List<int>();
	}
}