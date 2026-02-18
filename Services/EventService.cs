using EndguinField.Models;
namespace EndguinField.Services;

public interface IEventService
{
	Event? GetEventById(string eventId);
	List<Event> GetAllEvents();
}

public class EventService : IEventService
{
	private static List<Event> _events = new();

	public Event? GetEventById(string eventId)
	{
		return _events.FirstOrDefault(e => e.EventId == eventId);
	}

	public List<Event> GetAllEvents()
	{
		return _events;
	}
}

