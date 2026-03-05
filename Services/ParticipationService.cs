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
	Task<int> GetAcceptedParticipantCountAsync(string eventId);
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

	public async Task<bool> CancelParticipationAsync(string eventId, string userId)
	{
		var result = await _participationStore.DeleteAsync(eventId, userId);
		return result;
	}

	public async Task<bool> AcceptParticipationAsync(string eventId, string userId)
	{
		var result = await _participationStore.UpdateStatusAsync(eventId, userId, "Accepted");
		if (result)
		{
			var eventObj = await _eventStore.GetByIdAsync(eventId);
			if (eventObj != null)
			{
				await _notificationService.CreateNotificationAsync(
					userId,
					$"You were accepted to join {eventObj.Name}",
					"EventStatus",
					eventId
				);
			}
		}
		return result;
	}

	public async Task<bool> RejectParticipationAsync(string eventId, string userId)
	{
		var result = await _participationStore.UpdateStatusAsync(eventId, userId, "Rejected");
		if (result)
		{
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
		return result;
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
}
