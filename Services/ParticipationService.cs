using EndguinField.Models;
namespace EndguinField.Services;


public interface IParticipationService
{
	List<Participation> GetParticipationsByEvent(Event ev, IUserService userService);
}

public class ParticipationService : IParticipationService
{
	public List<Participation> GetParticipationsByEvent(Event ev, IUserService userService)
	{
		var participations = new List<Participation>();
		if (ev.Participants == null)
			return participations;

		foreach (var userId in ev.Participants)
		{
			var user = userService.GetUserById(userId);
			if (user != null)
			{
				// Try to find Participation in user's Enrolls for this event
				var participation = user.Enrolls.FirstOrDefault(p => p.EventId == ev.EventId);
				if (participation != null)
				{
					participations.Add(participation);
				}
				else
				{
					// Fallback: create a basic Participation if not found
					participations.Add(new Participation
					{
						UserId = user.Id,
						EventId = ev.EventId,
						User = user,
						JoinedTime = DateTime.MinValue,
						Status = "unknown"
					});
				}
			}
		}
		return participations;
	}
}