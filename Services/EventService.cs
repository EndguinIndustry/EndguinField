using EndguinField.Models;
using EndguinField.Models.ViewModels;
using EndguinField.Services.MongoDB;

namespace EndguinField.Services;

public interface IEventService
{
	Event? GetEventById(string eventId);
	List<Event> GetAllEvents();
	(bool success, string? error) CreateEvent(Event eventObj);
	void UpdateEvent(string id, Event eventObj);
	void DeleteEvent(string id);
	EventViewModel? GetEventViewModel(string id, string currentUserId);
}

public class EventService : IEventService
{
	private readonly IEventMongoService _eventStore;
	private readonly IUserService _userService;
	private readonly IParticipationService _participationService;
	private readonly ICommentService _commentService;

	public EventService(
		IEventMongoService eventStore,
		IUserService userService,
		IParticipationService participationService,
		ICommentService commentService)
	{
		_eventStore = eventStore;
		_userService = userService;
		_participationService = participationService;
		_commentService = commentService;
	}

	public List<Event> GetAllEvents()
	{
		return _eventStore.GetAllAsync().GetAwaiter().GetResult();
	}

	public Event? GetEventById(string id)
	{
		return _eventStore.GetByIdAsync(id).GetAwaiter().GetResult();
	}

	public (bool success, string? error) CreateEvent(Event eventObj)
	{
		if (eventObj.Images == null || eventObj.Images.Count < 1)
			return (false, "At least 1 image is required.");
		if (eventObj.Images.Count > 10)
			return (false, "Maximum 10 images allowed.");

		_eventStore.CreateAsync(eventObj).GetAwaiter().GetResult();
		return (true, null);
	}

	public void UpdateEvent(string id, Event eventObj)
	{
		_eventStore.UpdateAsync(id, eventObj).GetAwaiter().GetResult();
	}

	public void DeleteEvent(string id)
	{
		_eventStore.DeleteAsync(id).GetAwaiter().GetResult();
	}

	public EventViewModel? GetEventViewModel(string id, string currentUserId)
	{
		var thisEvent = GetEventById(id);
		if (thisEvent == null) return null;

		var owner = _userService.GetUserById(thisEvent.OwnerId);
		if (owner == null) return null;

		var participations = _participationService.GetParticipationsByEventId(thisEvent.Id);
		var participants = participations.Select(p =>
		{
			var user = _userService.GetUserById(p.UserId);
			if (user == null) return null;
			return new ParticipationViewModel
			{
				UserId = p.UserId,
				UserName = user.Name,
				UserImage = user.Image ?? string.Empty,
				Status = p.Status,
				JoinedTime = p.JoinedTime
			};
		}).Where(x => x != null).ToList()!;

		var comments = thisEvent.Comments.Select(cid =>
		{
			var c = _commentService.GetCommentById(cid);
			if (c == null) return null;
			var sender = _userService.GetUserById(c.SenderId);
			return new CommentViewModel
			{
				Id = c.Id,
				Content = c.Content,
				TimeStamp = c.TimeStamp,
				Sender = new UserSummaryViewModel
				{
					Id = sender?.Id ?? string.Empty,
					Name = sender?.Name ?? "Unknown",
					Image = sender?.Image ?? string.Empty
				},
				RepliedMessageId = c.RepliedMessageId
			};
		}).Where(c => c != null).ToList()!;

		return new EventViewModel
		{
			Id = thisEvent.Id,
			Name = thisEvent.Name,
			Description = thisEvent.Description,
			Images = thisEvent.Images,
			BackgroundImage = thisEvent.BackgroundImage,
			Location = thisEvent.Location,
			StartTime = thisEvent.StartTime,
			EndTime = thisEvent.EndTime,
			MaxParticipants = thisEvent.MaxParticipants,
			Category = thisEvent.Category,
			Status = thisEvent.Status,
			Owner = new UserSummaryViewModel
			{
				Id = owner.Id,
				Name = owner.Name,
				Image = owner.Image ?? string.Empty
			},
			Participants = participants,
			Comments = comments,
			IsOwner = thisEvent.OwnerId == currentUserId,
			IsJoined = participations.Any(p => p.UserId == currentUserId && p.Status == "Accepted"),
			UserStatus = participations.FirstOrDefault(p => p.UserId == currentUserId)?.Status
		};
	}
}
