using EndguinField.Models;
using EndguinField.Services.MongoDB;
namespace EndguinField.Services;

public interface IParticipationService
{
	Task<List<Participation>> GetParticipationsByEventIdAsync(string eventId);
	Task<bool> CancelParticipationAsync(string eventId, string userId);
	Task<bool> AcceptParticipationAsync(string eventId, string userId);
	Task<bool> RejectParticipationAsync(string eventId, string userId);
	Task<Participation?> GetParticipationAsync(string eventId, string userId);
	Task<Participation> JoinAsync(string eventId, string userId);
	Task<bool> DeleteParticipationsByEventIdAsync(string eventId);
}

public class ParticipationService : IParticipationService
{
	private readonly IParticipationMongoService _participationStore;
	private readonly IUserService _userService;

	public ParticipationService(IParticipationMongoService participationStore, IUserService userService)
	{
		_participationStore = participationStore;
		_userService = userService;
	}

	public async Task<List<Participation>> GetParticipationsByEventIdAsync(string eventId)
	{
		return await _participationStore.GetByEventIdAsync(eventId);
	}

	public async Task<bool> CancelParticipationAsync(string eventId, string userId)
	{
		return await _participationStore.DeleteAsync(eventId, userId);
	}

	public async Task<bool> AcceptParticipationAsync(string eventId, string userId)
	{
		return await _participationStore.UpdateStatusAsync(eventId, userId, "Accepted");
	}

	public async Task<bool> RejectParticipationAsync(string eventId, string userId)
	{
		return await _participationStore.UpdateStatusAsync(eventId, userId, "Rejected");
	}

	public async Task<Participation?> GetParticipationAsync(string eventId, string userId)
	{
		return await _participationStore.GetAsync(eventId, userId);
	}

	public async Task<Participation> JoinAsync(string eventId, string userId)
	{
		var user = await _userService.GetUserByIdAsync(userId);
		if (user == null)
		{
			throw new ArgumentException("User not found", nameof(userId));
		}

		var participation = new Participation
		{
			EventId = eventId,
			UserId = userId,
			Status = "Pending",
			JoinedTime = DateTime.UtcNow,
			User = user.Name
		};
		return await _participationStore.CreateAsync(participation);
	}

	public async Task<bool> DeleteParticipationsByEventIdAsync(string eventId)
	{
		return await _participationStore.DeleteByEventIdAsync(eventId);
	}
}
