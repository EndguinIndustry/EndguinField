using EndguinField.Models;
using EndguinField.Services.MongoDB;

namespace EndguinField.Services

{
	public interface IUserService
    {
        Task<User?> GetUserByIdAsync(string id);
        Task<List<User>> GetAllUsersAsync();
		Task<(bool success, string? error)> CreateUserAsync(User userData);
			Task<(bool success, string? error)> UpdateUserAsync(string id, User userData);
			Task<(bool success, string? error)> DeleteUserAsync(string id);
    }

	public class UserService : IUserService
	{
		private readonly IUserMongoService _userStore;

		public UserService(IUserMongoService userStore)
		{
			_userStore = userStore;
		}

		public async  Task<User?> GetUserByIdAsync(string id)
		{
			return await _userStore.GetByIdAsync(id);
		}

		public async Task<List<User>> GetAllUsersAsync()
		{
			return await _userStore.GetAllAsync();
		}

		public async Task<(bool success, string? error)> CreateUserAsync(User userData)
	{
			if (string.IsNullOrWhiteSpace(userData.Id))
			{
				userData.Id = Guid.NewGuid().ToString();
			}

			if (string.IsNullOrWhiteSpace(userData.Email) || string.IsNullOrWhiteSpace(userData.Name) || string.IsNullOrWhiteSpace(userData.Role))
		{
			return (false, "Email, Name, and role are required");
		}

			var existingUser = await _userStore.GetByIdAsync(userData.Id);
			if (existingUser != null)
			{
				return (false, "User id already exists");
			}


		await _userStore.CreateAsync(userData);
		
		return (true, null);
	}

			public async Task<(bool success, string? error)> UpdateUserAsync(string id, User userData)
		{
				var existingUser = await _userStore.GetByIdAsync(id);
				if (existingUser == null)
				{
					return (false, "User not found");
				}

				if (string.IsNullOrWhiteSpace(userData.Email) || string.IsNullOrWhiteSpace(userData.Name) || string.IsNullOrWhiteSpace(userData.Role))
				{
					return (false, "Email, Name, and role are required");
				}

				userData.Id = id;
				await _userStore.UpdateAsync(id, userData);
				return (true, null);
		}

			public async Task<(bool success, string? error)> DeleteUserAsync(string id)
			{
				var existingUser = await _userStore.GetByIdAsync(id);
				if (existingUser == null)
				{
					return (false, "User not found");
				}

				await _userStore.DeleteAsync(id);
				return (true, null);
			}
	}
}