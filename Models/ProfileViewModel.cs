namespace EndguinField.Models
{

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
        public int FriendsCount { get; set; }
        public int EnrollsCount { get; set; }
        public bool IsBanned { get; set; }
        public DateTime? BanUntil { get; set; }
        public int PendingFriendRequests { get; set; }

        public static ProfileViewModel FromUser(User user)
        {
            return new ProfileViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                Bio = user.Bio,
                ImageUrl = user.Image,
                Contact = user.Contact,
                JoinedDate = user.JoinWhen.ToString("dd-MM-yyyy", new System.Globalization.CultureInfo("en-US")),
                FriendsCount = user.Friends?.Count ?? 0,
                EnrollsCount = user.Enrolls?.Count ?? 0,
                IsBanned = user.BanUntil.HasValue && user.BanUntil.Value > DateTime.Now,
                BanUntil = user.BanUntil,
                PendingFriendRequests = user.FriendRequests?.Count(fr => fr.Status == "Pending") ?? 0
            };
        }
    }
}