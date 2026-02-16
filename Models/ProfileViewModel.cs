using System.Globalization;
using EndguinField.Services;
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
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
    }

    public class ProfileViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? ImageUrl { get; set; }
        public string? Contact { get; set; }
        public string JoinedDate { get; set; } = string.Empty;
        public List<int> FriendIds { get; set; } = new();
        public List<FriendViewModel> Friends { get; set; } = new();
        public int EnrollsCount { get; set; }
        public bool IsBanned { get; set; }
        public DateTime? BanUntil { get; set; }
        public List<int> PendingFriendRequestIds { get; set; } = new();
        public List<FriendViewModel> PendingRequests { get; set; } = new();

        public static ProfileViewModel FromUser(User user, IUserService userService)
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
                JoinedDate = user.JoinWhen.ToString("dd-MM-yyyy", new CultureInfo("en-US")),

                FriendIds = user.Friends ?? new List<int>(),
                PendingFriendRequestIds = user.FriendRequests ?? new List<int>(),

                EnrollsCount = user.Enrolls?.Count ?? 0,
                IsBanned = user.BanUntil.HasValue && user.BanUntil.Value > DateTime.Now,
                BanUntil = user.BanUntil
            };
            model.Friends = model.FriendIds
                .Select(id => userService.GetUserById(id))
                .Where(f => f != null)
                .Select(f => new FriendViewModel
                {
                    Id = f!.Id,
                    Name = f.Name,
                    ImageUrl = f.Image
                }).ToList();

            model.PendingRequests = model.PendingFriendRequestIds
                .Select(id => userService.GetUserById(id))
                .Where(f => f != null)
                .Select(f => new FriendViewModel
                {
                    Id = f!.Id,
                    Name = f.Name,
                    ImageUrl = f.Image
                }).ToList();

            return model;
        }
    }

    public class UpdateProfileDTO
    {
        public string Name { get; set; } = string.Empty;
        public string? Contact { get; set; }
        public string? Bio { get; set; }
    }
}