using System.Globalization;
using EndguinField.Services;
using System.ComponentModel.DataAnnotations;
namespace EndguinField.Models
{
	public class HistoryEventViewModel
	{
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string ImgSrc { get; set; } = string.Empty;
		public string Date { get; set; } = string.Empty;
		public string Location { get; set; } = string.Empty;
	}
	public class FriendViewModel
	{
		public string Id { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public string? ImageUrl { get; set; }
	}

	public class ProfileViewModel
	{
		public string Id { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string Role { get; set; } = string.Empty;
		public string? Bio { get; set; }
		public string? ImageUrl { get; set; }
		public string? Contact { get; set; }
		public string? Gender { get; set; }
		public string JoinedDate { get; set; } = string.Empty;
		public List<string> FriendIds { get; set; } = new();
		public List<FriendViewModel> Friends { get; set; } = new();
		public int EnrollsCount { get; set; }
		public bool IsBanned { get; set; }
		public DateTime? BanUntil { get; set; }
		public List<string> PendingFriendRequestIds { get; set; } = new();
		public List<FriendViewModel> PendingRequests { get; set; } = new();

		public static async Task<ProfileViewModel> FromUserAsync(User user, IUserService userService)
		{

			var model = new ProfileViewModel
			{
				Id = user.Id,
				Name = user.Name,
				Email = user.Email,
				Role = user.Role,
				Bio = user.Bio,
				ImageUrl = user.Image,
				Contact = user.Contact,
				Gender = user.Gender,
				JoinedDate = user.JoinWhen.ToString("dd-MM-yyyy", new CultureInfo("en-US")),

				FriendIds = user.Friends ?? new List<string>(),
				PendingFriendRequestIds = user.FriendRequests ?? new List<string>(),

				EnrollsCount = user.Enrolls?.Count ?? 0,
				IsBanned = user.BanUntil.HasValue && user.BanUntil.Value > DateTime.Now,
				BanUntil = user.BanUntil
			};
			var friendTasks = model.FriendIds
				.Select(id => userService.GetUserByIdAsync(id));
			var friends = await Task.WhenAll(friendTasks);

			model.Friends = friends
				.Where(f => f != null)
				.Select(f => new FriendViewModel
				{
					Id = f!.Id,
					Name = f.Name,
					ImageUrl = f.Image
				})
				.ToList();

			var pendingTasks = model.PendingFriendRequestIds
				.Select(id => userService.GetUserByIdAsync(id));
			var pending = await Task.WhenAll(pendingTasks);

			model.PendingRequests = pending
				.Where(f => f != null)
				.Select(f => new FriendViewModel
				{
					Id = f!.Id,
					Name = f.Name,
					ImageUrl = f.Image
				})
				.ToList();

			return model;
		}
	}

	public class UserUpdateModel
    {
        public string? Id { get; set; }

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        public string Role { get; set; } = "User";
        public string? Bio { get; set; }
        public string? Contact { get; set; }
		public string? Gender { get; set; }

        public IFormFile? ProfileImageFile { get; set; }

        public User ToUserModel()
        {
            return new User
            {
                Id = string.IsNullOrEmpty(Id) ? Guid.NewGuid().ToString() : Id,
                Email = Email,
                Name = Name,
                Role = Role,
                Bio = Bio,
                Contact = Contact,
				Gender = Gender,
                JoinWhen = DateTime.UtcNow,
                Friends = new(),
                BlockLists = new(),
                Enrolls = new(),
                FriendRequests = new(),
                BanUntil = null
            };
        }
    }

	public class UpdateProfileDTO
	{
		public string Name { get; set; } = string.Empty;
		public string? Contact { get; set; }
		public string? Bio { get; set; }
		public string? Gender { get; set; }
	}
}