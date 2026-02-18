using EndguinField.Models;
namespace EndguinField.Services

{
	public interface IUserService
	{
		User? GetUserById(string id);
		List<User> GetAllUsers();
	}

	public class UserService : IUserService
	{
		private static List<User> _users = new();

		public UserService()
		{
			if (_users.Count == 0)
			{
				InitializeMockData();
			}
		}

		public User? GetUserById(string id)
		{
			return _users.FirstOrDefault(u => u.Id == id);
		}

		public List<User> GetAllUsers()
		{
			return _users;
		}

		private void InitializeMockData()
		{
			_users = new List<User>
			{
				new User
				{
					Id = "1",
					Email = "chen.qianyu@endguin.com",
					Name = "Chen Qianyu",
					Role = "Admin",
					Bio = "EndguinField is a platform for writing and dancing with Chen. And I am user number 1 of EndguinField.",
					Image = "https://preview.redd.it/the-origin-of-chen-sticker-v0-aulsmkoceg0g1.png?width=300&format=png&auto=webp&s=9578b04c0a2fa42f1472ed9bb29b07651ee5effd",
					Contact = "+1234567890",
					JoinWhen = new DateTime(2026, 2, 14),
					Friends = new List<int> {2,3,4,5},
					Enrolls = new List<Participation>(),
					FriendRequests = new List<int>{},
					BanUntil = null
				},
				new User
				{
					Id = "2",
					Email = "ayaka.kamisato@inazuma.com",
					Name = "Ayaka Kamisato",
					Role = "User",
					Bio = "Close your eyes and face the wind... It feels wonderful, doesn't it?",
					Image = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSNI8DM4P98gl2Q9AAhD6XlQKS0oaxKsVcJ0w&s",
					Contact = "+0987654321",
					JoinWhen = new DateTime(2026, 1, 15),
					Friends = new List<int> {1},
					Enrolls = new List<Participation>(),
					FriendRequests = new List<int>(),
					BanUntil = null
				},
				new User
				{
					Id = "3",
					Email = "raiden.shogun@inazuma.com",
					Name = "Raiden Shogun",
					Role = "Moderator",
					Bio = "Eternity is the only truth in this world.",
					Image = "https://pbs.twimg.com/media/E9wglivVEAgUV8r.jpg",
					Contact = "+1122334455",
					JoinWhen = new DateTime(2026, 1, 15),
					Friends = new List<int> {1},
					Enrolls = new List<Participation>(),
					FriendRequests = new List<int>(),
					BanUntil = null
				},
				new User
				{
					Id = "4",
					Email = "yoimiya@inazuma.com",
					Name = "Yoimiya Naganohara",
					Role = "User",
					Bio = "Let's light up the sky with fireworks!",
					Image = "https://media.tenor.com/nfYv2UZADtgAAAAe/genshin-yoimiya.png",
					Contact = "+5566778899",
					JoinWhen = new DateTime(2026, 1, 15),
					Friends = new List<int> {1},
					Enrolls = new List<Participation>(),
					FriendRequests = new List<int>(),
					BanUntil = null
				},
				new User
				{
					Id = "5",
					Email = "kazuha@inazuma.com",
					Name = "Kaedehara Kazuha",
					Role = "User",
					Bio = "Come driving rain or winds that churn, I shall return, by blade alone.",
					Image = "https://i.redd.it/59gd146dye3b1.jpg",
					Contact = "+9988776655",
					JoinWhen = new DateTime(2025, 11, 15),
					Friends = new List<int> {1},
					Enrolls = new List<Participation>(),
					FriendRequests = new List<int>(),
					BanUntil = new DateTime(2026, 3, 1)
				}
			};
		}
	}
}