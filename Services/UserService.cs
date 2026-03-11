using EndguinField.Models;
using EndguinField.Services.MongoDB;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace EndguinField.Services

{
	public interface IUserService
	{
		string? GetCurrentUserId();
		Task<bool> IsAdminAsync(string userId);
        Task<User?> GetUserByIdAsync(string id);
        Task<List<User>> GetAllUsersAsync();
		Task<(bool success, string? error)> CreateUserAsync(User userData);
		Task<(bool success, string? error)> UpdateUserAsync(string id, User userData);
		Task<(bool success, string? error)> DeleteUserAsync(string id);
		Task<List<String>> GetAllPendingEventAsync(string userId);
		Task<List<String>> GetAllParticipatingEventAsync(string userId);
		Task<List<String>> GetAllMyOngoingEventAsync(string userId);
		Task<List<String>> GetAllPastEventAsync(string userId);
		Task<List<String>> GetAllMyEventAsync(string userId);
	}

	public class UserService : IUserService
	{
		private readonly IUserMongoService _userStore;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly IParticipationMongoService _participationStore;
		private readonly IEventMongoService _eventStore;

		public UserService(IUserMongoService userStore, IHttpContextAccessor httpContextAccessor, IParticipationMongoService participationStore, IEventMongoService eventStore)
		{
			_userStore = userStore;
			_httpContextAccessor = httpContextAccessor;
			_participationStore = participationStore;
			_eventStore = eventStore;
		}

		public string? GetCurrentUserId()
		{
			return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
		}

		public async Task<User?> GetUserByIdAsync(string id)
		{
			return await _userStore.GetByIdAsync(id);
		}

		public async Task<bool> IsAdminAsync(string userId)
		{
			if (string.IsNullOrEmpty(userId)) return false;
			var user = await _userStore.GetByIdAsync(userId);
			return user != null && user.Role == "Admin";
		}

		public async Task<List<User>> GetAllUsersAsync()
		{
			return await _userStore.GetAllAsync();
		}

		public async Task<List<String>> GetAllMyOngoingEventAsync(string userId)
		{
			var events = await _eventStore.GetAllAsync();

			var matchedEventIds = events
				.Where(e => e.OwnerId == userId && DateTime.Now <= e.StartTime && e.EndTime >= DateTime.Now)
				.Select(e => e.Id)
				.Distinct()
				.ToList();

			return matchedEventIds;
		}

		public async Task<List<String>> GetAllMyEventAsync(string userId)
		{
			var events = await _eventStore.GetAllAsync();

			var matchedEventIds = events
				.Where(e => e.OwnerId == userId)
				.Select(e => e.Id)
				.Distinct()
				.ToList();

			return matchedEventIds;
		}

		public async Task<List<String>> GetAllPendingEventAsync(string userId)
		{
			var participations = await _participationStore.GetByUserIdAsync(userId);
			var matchedEventIds = participations
				.Where(p => p.Status == "Pending" && DateTime.Now < _eventStore.GetByIdAsync(p.EventId).GetAwaiter().GetResult().StartTime)
				.Select(p => p.EventId)
				.Distinct()
				.ToList();

			var events = new List<String>();
			foreach (var eventId in matchedEventIds)
			{
				events.Add(eventId);
			}

			return events;
		}

		public async Task<List<String>> GetAllParticipatingEventAsync(string userId)
		{
			var participations = await _participationStore.GetByUserIdAsync(userId);
			var matchedEventIds = participations
				.Where(p => p.Status == "Accepted" && _eventStore.GetByIdAsync(p.EventId).GetAwaiter().GetResult().EndTime >= DateTime.Now)
				.Select(p => p.EventId)
				.Distinct()
				.ToList();

			var events = new List<String>();
			foreach (var eventId in matchedEventIds)
			{
				events.Add(eventId);
			}

			return events;
		}

		public async Task<List<String>> GetAllPastEventAsync(string userId)
		{
			var participations = await _participationStore.GetByUserIdAsync(userId);
			var matchedEventIds = participations
				.Where(p =>
				{
					return
					p.Status == "Accepted" &&
					_eventStore.GetByIdAsync(p.EventId).GetAwaiter().GetResult().EndTime < DateTime.Now;
				}
				)
				.Select(p => p.EventId)
				.Distinct()
				.ToList();

			var events = new List<String>();
			foreach (var eventId in matchedEventIds)
			{
				events.Add(eventId);
				var eventData = await _eventStore.GetByIdAsync(eventId);
			}
			return events;
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