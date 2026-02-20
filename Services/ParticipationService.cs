using EndguinField.Models;
using EndguinField.Services.MongoDB;
namespace EndguinField.Services;

public interface IParticipationService
{
	List<Participation> GetParticipationsByEventId(string eventId);
	bool CancelParticipation(string eventId, string userId);
	bool AcceptParticipation(string eventId, string userId);
	bool RejectParticipation(string eventId, string userId);
	Participation? GetParticipation(string eventId, string userId);
	Participation Join(string eventId, string userId);
	bool DeleteParticipationsByEventId(string eventId);
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

	public List<Participation> GetParticipationsByEventId(string eventId)
	{
		return _participationStore.GetByEventIdAsync(eventId).GetAwaiter().GetResult();
	}

	public bool CancelParticipation(string eventId, string userId)
	{
		return _participationStore.DeleteAsync(eventId, userId).GetAwaiter().GetResult();
	}

	public bool AcceptParticipation(string eventId, string userId)
	{
		return _participationStore.UpdateStatusAsync(eventId, userId, "Accepted").GetAwaiter().GetResult();
	}

	public bool RejectParticipation(string eventId, string userId)
	{
		return _participationStore.UpdateStatusAsync(eventId, userId, "Rejected").GetAwaiter().GetResult();
	}

	public Participation? GetParticipation(string eventId, string userId)
	{
		return _participationStore.GetAsync(eventId, userId).GetAwaiter().GetResult();
	}

	public Participation Join(string eventId, string userId)
	{
		var user = _userService.GetUserById(userId);
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
		return _participationStore.CreateAsync(participation).GetAwaiter().GetResult();
	}

	public bool DeleteParticipationsByEventId(string eventId)
	{
		return _participationStore.DeleteByEventIdAsync(eventId).GetAwaiter().GetResult();
	}
}
