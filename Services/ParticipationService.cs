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
	Task RemovePendingParticipationsAsync(string eventId);
}

public class ParticipationService : IParticipationService
{
	private readonly IParticipationMongoService _participationStore;
	private readonly IUserService _userService;
	private readonly IEventMongoService _eventStore;
	private readonly INotificationService _notificationService;

	public ParticipationService(IParticipationMongoService participationStore, IUserService userService, IEventMongoService eventStore, INotificationService notificationService)
	{
		_participationStore = participationStore;
		_userService = userService;
		_eventStore = eventStore;
		_notificationService = notificationService;
	}

	public async Task<List<Participation>> GetParticipationsByEventIdAsync(string eventId)
	{
		return await _participationStore.GetByEventIdAsync(eventId);
	}

	public async Task CancelParticipationAsync(string eventId, string userId, string currentUserId)
	{
		var cancelEventData = await _eventStore.GetByIdAsync(eventId);
		if (cancelEventData == null)
		{
			throw new KeyNotFoundException("Event not found.");
		}

		if (cancelEventData.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
		{
			throw new InvalidOperationException("You cannot cancel participation in a cancelled event.");
		}

		if (DateTime.Now > cancelEventData.EndTime)
		{
			throw new InvalidOperationException("This event has already ended.");
		}

		if (userId != currentUserId && cancelEventData.OwnerId != currentUserId)
		{
			throw new UnauthorizedAccessException("Forbidden");
		}

		if (userId == cancelEventData.OwnerId)
		{
			throw new InvalidOperationException("Owner cannot quit their own event.");
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

		if (eventData.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
		{
			throw new InvalidOperationException("You cannot manage participants in a cancelled event.");
		}

		if (DateTime.Now > eventData.EndTime)
		{
			throw new InvalidOperationException("This event has already ended.");
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

		if (eventData.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
		{
			throw new InvalidOperationException("You cannot manage participants in a cancelled event.");
		}

		if (DateTime.Now > eventData.EndTime)
		{
			throw new InvalidOperationException("This event has already ended.");
		}

		var result = await _participationStore.UpdateStatusAsync(eventId, userId, "Rejected");
		if (!result)
		{
			throw new InvalidOperationException("Failed to decline join request.");
		} else {
			var eventObj = await _eventStore.GetByIdAsync(eventId);
			if (eventObj != null)
			{
				await _notificationService.CreateNotificationAsync(
					userId,
					$"You were rejected from joining {eventObj.Name}",
					"EventStatus",
					eventId
				);
			}
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

		if (eventData.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
		{
			throw new InvalidOperationException("You cannot join a cancelled event.");
		}

		if (DateTime.Now > eventData.EndTime)
		{
			throw new InvalidOperationException("This event has already ended.");
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

		var eventObj = await _eventStore.GetByIdAsync(eventId);

		var participation = new Participation
		{
			EventId = eventId,
			UserId = userId,
			Status = "Pending",
			JoinedTime = DateTime.UtcNow,
			User = user.Name
		};

		await _participationStore.CreateAsync(participation);

		if (eventObj != null)
		{
			await _notificationService.CreateNotificationAsync(
				eventObj.OwnerId,
				$"{user.Name} requested to join your event {eventObj.Name}",
				"EventRequest",
				eventId
			);
		}

		return participation;
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

	public async Task RemovePendingParticipationsAsync(string eventId)
	{
		var participations = await _participationStore.GetByEventIdAsync(eventId);
		var pendingParticipations = participations.Where(p => string.Equals(p.Status, "Pending", StringComparison.OrdinalIgnoreCase));
		foreach (var participation in pendingParticipations)
		{
			await _participationStore.DeleteAsync(participation.EventId, participation.UserId);
		}
	}
}
