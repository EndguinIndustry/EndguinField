using EndguinField.Models.ViewModels;


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
		public List<string> Friends { get; set; } = new List<string>();
		public List<string> BlockLists { get; set; } = new List<string>();
		public List<string> Enrolls { get; set; } = new List<string>();
		public DateTime? BanUntil { get; set; }
		public List<string> FriendRequests { get; set; } = new List<string>();

		public UserSummaryViewModel ToSummaryViewModel()
		{
			return new UserSummaryViewModel
			{
				Id = Id,
				Name = Name,
				Image = Image ?? string.Empty
			};
		}
	}
}