using EndguinField.Models;
using EndguinField.Models.ViewModels;
using EndguinField.Services.MongoDB;

namespace EndguinField.Services;

public interface IEventService
{
	Task<Event?> GetEventByIdAsync(string eventId);
	Task<List<Event>> GetAllEventsAsync();
	Task<List<Event>> GetAllEventsWithParticipantsAsync();
	Task<Event> CreateEventAsync(Event eventData);
	Task UpdateEventAsync(string eventId, Event eventData, string currentUserId);
	Task DeleteEventAsync(string eventId, string currentUserId);
	Task AddCommentAsync(string eventId, string senderId, string content, string? repliedMessageId = null);
	Task<EventViewModel?> GetEventViewModelAsync(string eventId, string currentUserId);
	Task<string> GetEventDisplayStatusAsync(string eventId);
}

public class EventService : IEventService
{
	private readonly IEventMongoService _eventStore;
	private readonly IUserService _userService;
	private readonly IParticipationService _participationService;
	private readonly ICommentService _commentService;
    private readonly INotificationService _notificationService;

	public EventService(
		IEventMongoService eventStore,
		IUserService userService,
		IParticipationService participationService,
		ICommentService commentService,
        INotificationService notificationService)
	{
		_eventStore = eventStore;
		_userService = userService;
		_participationService = participationService;
		_commentService = commentService;
        _notificationService = notificationService;
	}

	public async Task<List<Event>> GetAllEventsAsync()
	{
		return await _eventStore.GetAllAsync();
	}

	public async Task<List<Event>> GetAllEventsWithParticipantsAsync()
	{
		var events = await _eventStore.GetAllAsync();
		foreach (var evt in events)
		{
			var participations = await _participationService.GetParticipationsByEventIdAsync(evt.Id);
			evt.Participants = participations
				.Where(p => p.Status == "Accepted") 
				.Select(p => p.UserId)
				.ToList();
		}
		return events;
	}

	public async Task<Event?> GetEventByIdAsync(string eventId)
	{
		var ev = await _eventStore.GetByIdAsync(eventId);
		if (ev != null)
		{
			var displayStatus = ev.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase)
				? "Cancelled"
				: (DateTime.Now > ev.EndTime ? "Ended" : "Open");
			if (displayStatus == "Ended")
			{
				await _participationService.RemovePendingParticipationsAsync(eventId);
			}
		}
		return ev;
	}

	public async Task<Event> CreateEventAsync(Event eventData)
	{
		var (isValid, errorMessage) = checkValidEvent(eventData);
		if (!isValid)
		{
			throw new ArgumentException(errorMessage ?? "Invalid event data.");
		}

		await _eventStore.CreateAsync(eventData);
		
		return eventData;
	}

	public async Task UpdateEventAsync(string eventId, Event eventData, string currentUserId)
	{
		var existingEvent = await _eventStore.GetByIdAsync(eventId);
		if (existingEvent == null)
		{
			throw new KeyNotFoundException("Event not found.");
		}

		var displayStatus = await GetEventDisplayStatusAsync(eventId);
		if (displayStatus != "Open")
		{
			bool isAdminAction = await _userService.IsAdminAsync(currentUserId);
			if (!(isAdminAction && eventData.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase)))
			{
				throw new InvalidOperationException("Cannot update an event that is already ended or cancelled.");
			}
		}

		bool isAdmin = await _userService.IsAdminAsync(currentUserId);
		if (existingEvent.OwnerId != currentUserId && !isAdmin)
		{
			throw new UnauthorizedAccessException("Forbidden.");
		}

		var (isValid, errorMessage) = checkValidEvent(eventData);
		if (!isValid)
		{
			throw new ArgumentException(errorMessage ?? "Invalid event data.");
		}

		var participations = await _participationService.GetParticipationsByEventIdAsync(eventId);
		int acceptedCount = participations.Count(participation => participation.Status == "Accepted");

		if (eventData.MaxParticipants < acceptedCount)
		{
			throw new ArgumentException($"Cannot set max participants below current accepted count ({acceptedCount}).");
		}

		await _eventStore.UpdateAsync(eventId, eventData);
		
		foreach (var p in participations.Where(p => p.Status == "Accepted" || p.Status == "Pending"))
        {
            await _notificationService.CreateNotificationAsync(
                p.UserId,
                $"Event '{eventData.Name}' was updated.",
                "EventUpdate",
                eventId
            );
        }
	}

	public async Task DeleteEventAsync(string eventId, string currentUserId)
	{
		var eventData = await GetEventByIdAsync(eventId);

		if (eventData == null)
		{
			throw new KeyNotFoundException("Event not found.");
		}

		bool isAdmin = await _userService.IsAdminAsync(currentUserId);
		if (eventData.OwnerId != currentUserId && !isAdmin)
		{
			throw new UnauthorizedAccessException("Forbidden.");
		}

		await _participationService.DeleteParticipationsByEventIdAsync(eventId);
		await _commentService.DeleteCommentsAsync(eventData.Comments);
		await _eventStore.DeleteAsync(eventId);
	}

	public async Task AddCommentAsync(string eventId, string senderId, string content, string? repliedMessageId = null)
	{
		var eventData = await GetEventByIdAsync(eventId);

		if (eventData == null)
		{
			throw new KeyNotFoundException("Event not found.");
		}

		var comment = new Comment
		{
			Id = Guid.NewGuid().ToString(),
			SenderId = senderId,
			Content = content,
			TimeStamp = DateTime.Now,
			RepliedMessageId = repliedMessageId
		};

		await _commentService.CreateCommentAsync(comment);
		eventData.Comments.Add(comment.Id);
		await _eventStore.UpdateAsync(eventId, eventData);
	}

	public async Task<string> GetEventDisplayStatusAsync(string eventId)
	{
		var eventData = await GetEventByIdAsync(eventId);
		if (eventData == null)
		{
			throw new KeyNotFoundException("Event not found.");	
		}

		return eventData.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase) 
			? "Cancelled" 
			: (DateTime.Now > eventData.EndTime ? "Ended" : "Open");
	}

	public async Task<EventViewModel?> GetEventViewModelAsync(string eventId, string currentUserId)
	{
		var eventData = await GetEventByIdAsync(eventId);

		if (eventData == null)
		{
			return null;
		}

		if (await GetEventDisplayStatusAsync(eventId) == "Ended")
		{
			await _participationService.RemovePendingParticipationsAsync(eventId);
		}

		var owner = await _userService.GetUserByIdAsync(eventData.OwnerId);

		if (owner == null)
		{
			return null;
		}

		var participations = await _participationService.GetParticipationsByEventIdAsync(eventData.Id);
		var userParticipation = participations.FirstOrDefault(participation => participation.UserId == currentUserId);

		bool isOwner = eventData.OwnerId == currentUserId;

		var displayedParticipations = isOwner
			? participations
			: participations.Where(p => string.Equals(p.Status, "Accepted", StringComparison.OrdinalIgnoreCase)).ToList();

		return new EventViewModel
		{
			Id = eventData.Id,
			Name = eventData.Name,
			Description = eventData.Description,
			Images = eventData.Images,
			BackgroundImage = eventData.BackgroundImage,
			Location = eventData.Location,
			StartTime = eventData.StartTime,
			EndTime = eventData.EndTime,
			MaxParticipants = eventData.MaxParticipants,
			Tags = eventData.Tags,
			Status = eventData.Status,
			DisplayStatus = await GetEventDisplayStatusAsync(eventData.Id),
			Owner = owner.ToSummaryViewModel(),
			Participants = await MapParticipants(displayedParticipations),
			ParticipationCount = await _participationService.GetAcceptedParticipantCountAsync(eventData.Id),
			Comments = await MapCommentsAsync(eventData.Comments),
			IsOwner = isOwner,
			IsJoined = userParticipation?.Status?.Equals("Accepted", StringComparison.OrdinalIgnoreCase) == true,
			UserStatus = userParticipation?.Status
		};
	}

	private (bool success, string? error) checkValidEvent(Event eventData)
	{
		if (string.IsNullOrWhiteSpace(eventData.Name) || eventData.Name.Length > 100)
		{
			return (false, "Event name is required and must be 100 characters or less.");
		}

		if (eventData.Description?.Length > 2000)
		{
			return (false, "Description must be 2000 characters or less.");
		}

		if (eventData.Location?.Length > 500)
		{
			return (false, "Location must be 500 characters or less.");
		}

		if (eventData.MaxParticipants < 1 || eventData.MaxParticipants > 100)
		{
			return (false, "Max participants must be between 1 and 100.");
		}

		if (eventData.Images == null || eventData.Images.Count < 1)
		{
			return (false, "At least 1 image is required.");
		}

		if (eventData.Images.Count > 10)
		{
			return (false, "Maximum 10 images allowed.");
		}

		if (eventData.EndTime <= eventData.StartTime)
		{
			return (false, "End date and time must be after the start date and time.");
		}

		return (true, null);
	}

	private async Task<List<ParticipationViewModel>> MapParticipants(List<Participation> participations)
	{
		var list = new List<ParticipationViewModel>();

		foreach (var participation in participations)
		{
			var user = await _userService.GetUserByIdAsync(participation.UserId);
			if (user != null)
			{
				list.Add(participation.ToViewModel(user.ToSummaryViewModel()));
			}
		}

		return list;
	}

	private async Task<List<CommentViewModel>> MapCommentsAsync(List<string> commentIds)
	{
		var tasks = commentIds.Select(async commentId =>
		{
			var comment = await _commentService.GetCommentByIdAsync(commentId);
			if (comment == null)
			{
				return null;
			}

			var sender = await _userService.GetUserByIdAsync(comment.SenderId);
			return comment.ToViewModel(sender?.ToSummaryViewModel()!);
		});

		var results = await Task.WhenAll(tasks);

		return results
			.OfType<CommentViewModel>()
			.ToList();
	}
}
