using EndguinField.Models;
using EndguinField.Services.MongoDB;
namespace EndguinField.Services;

public interface IParticipationService
{
	Task<List<Participation>> GetParticipationsByEventIdAsync(string eventId);
	Task CancelParticipationAsync(string eventId, string userId, string currentUserId);
	Task AcceptParticipationAsync(string eventId, string userId, string currentUserId);
	Task RejectParticipationAsync(string eventId, string userId, string currentUserId);
	Task<Participation?> GetParticipationAsync(string eventId, string userId);
	Task<Participation> JoinAsync(string eventId, string userId);
	Task<bool> DeleteParticipationsByEventIdAsync(string eventId);
	Task<int> GetAcceptedParticipantCountAsync(string eventId);
}

public class ParticipationService : IParticipationService
{
	private readonly IParticipationMongoService _participationStore;
	private readonly IUserService _userService;
	private readonly IEventMongoService _eventStore;

	public ParticipationService(IParticipationMongoService participationStore, IUserService userService, IEventMongoService eventStore)
	{
		_participationStore = participationStore;
		_userService = userService;
		_eventStore = eventStore;
	}

	public async Task<List<Participation>> GetParticipationsByEventIdAsync(string eventId)
	{
		return await _participationStore.GetByEventIdAsync(eventId);
	}

	public async Task CancelParticipationAsync(string eventId, string userId, string currentUserId)
	{
		if (userId != currentUserId)
		{
			var eventData = await _eventStore.GetByIdAsync(eventId);
			if (eventData == null || eventData.OwnerId != currentUserId)
			{
				throw new UnauthorizedAccessException("Forbidden");
			}
		}

		var result = await _participationStore.DeleteAsync(eventId, userId);
		if (!result)
		{
			throw new InvalidOperationException("Failed to cancel participation.");
		}
	}

	public async Task AcceptParticipationAsync(string eventId, string userId, string currentUserId)
	{
		var eventData = await _eventStore.GetByIdAsync(eventId);
		if (eventData == null || eventData.OwnerId != currentUserId)
		{
			throw new UnauthorizedAccessException("Forbidden");
		}

		var participations = await _participationStore.GetByEventIdAsync(eventId);
		int acceptedCount = participations.Count(p => string.Equals(p.Status, "Accepted", StringComparison.OrdinalIgnoreCase));

		if (acceptedCount >= eventData.MaxParticipants)
		{
			throw new InvalidOperationException($"Cannot accept. Event has reached its maximum capacity of {eventData.MaxParticipants} participants.");
		}

		var result = await _participationStore.UpdateStatusAsync(eventId, userId, "Accepted");
		if (!result)
		{
			throw new InvalidOperationException("Failed to accept join request.");
		}
	}

	public async Task RejectParticipationAsync(string eventId, string userId, string currentUserId)
	{
		var eventData = await _eventStore.GetByIdAsync(eventId);
		if (eventData == null || eventData.OwnerId != currentUserId)
		{
			throw new UnauthorizedAccessException("Forbidden");
		}

		var result = await _participationStore.UpdateStatusAsync(eventId, userId, "Rejected");
		if (!result)
		{
			throw new InvalidOperationException("Failed to decline join request.");
		}
	}

	public async Task<Participation?> GetParticipationAsync(string eventId, string userId)
	{
		return await _participationStore.GetAsync(eventId, userId);
	}

	public async Task<Participation> JoinAsync(string eventId, string userId)
	{
		var eventData = await _eventStore.GetByIdAsync(eventId);
		if (eventData == null)
		{
			throw new KeyNotFoundException("Event not found.");
		}

		if (eventData.OwnerId == userId)
		{
			throw new InvalidOperationException("You cannot join your own event.");
		}

		var existingParticipation = await _participationStore.GetAsync(eventId, userId);
		if (existingParticipation != null)
		{
			throw new InvalidOperationException("You have already requested to join or have already joined this event.");
		}

		var participations = await _participationStore.GetByEventIdAsync(eventId);
		int acceptedCount = participations.Count(p => string.Equals(p.Status, "Accepted", StringComparison.OrdinalIgnoreCase));

		if (acceptedCount >= eventData.MaxParticipants)
		{
			throw new InvalidOperationException($"This event has reached its maximum capacity of {eventData.MaxParticipants} participants.");
		}

		var user = await _userService.GetUserByIdAsync(userId);
		if (user == null)
		{
			throw new KeyNotFoundException("User not found.");
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

	public async Task<int> GetAcceptedParticipantCountAsync(string eventId)
	{
		var participations = await _participationStore.GetByEventIdAsync(eventId);
		return participations.Count(p => string.Equals(p.Status, "Accepted", StringComparison.OrdinalIgnoreCase));
	}
}
