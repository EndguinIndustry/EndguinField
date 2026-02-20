using EndguinField.Models;
using EndguinField.Models.ViewModels;
using EndguinField.Services.MongoDB;

namespace EndguinField.Services;

public interface IEventService
{
	Task<Event?> GetEventByIdAsync(string eventId);
	Task<List<Event>> GetAllEventsAsync();
	Task<(bool success, string? error)> CreateEventAsync(Event eventData);
	Task<(bool success, string? error)> UpdateEventAsync(string eventId, Event eventData);
	Task DeleteEventAsync(string eventId);
	Task<(bool success, string? error)> AddCommentAsync(string eventId, string senderId, string content, string? repliedMessageId = null);
	Task<EventViewModel?> GetEventViewModelAsync(string eventId, string currentUserId);
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

	public async Task<List<Event>> GetAllEventsAsync()
	{
		return await _eventStore.GetAllAsync();
	}

	public async Task<Event?> GetEventByIdAsync(string eventId)
	{
		return await _eventStore.GetByIdAsync(eventId);
	}

	public async Task<(bool success, string? error)> CreateEventAsync(Event eventData)
	{
		if (eventData.Images == null || eventData.Images.Count < 1)
		{
			return (false, "At least 1 image is required.");
		}

		if (eventData.Images.Count > 10)
		{
			return (false, "Maximum 10 images allowed.");
		}

		await _eventStore.CreateAsync(eventData);
		
		return (true, null);
	}

	public async Task<(bool success, string? error)> UpdateEventAsync(string eventId, Event eventData)
	{
		var participations = await _participationService.GetParticipationsByEventIdAsync(eventId);
		int acceptedCount = participations.Count(participation => participation.Status == "Accepted");

		if (eventData.MaxParticipants < acceptedCount)
		{
			return (false, $"Cannot set max participants below current accepted count ({acceptedCount}).");
		}

		await _eventStore.UpdateAsync(eventId, eventData);
		
		return (true, null);
	}

	public async Task DeleteEventAsync(string eventId)
	{
		var eventData = await GetEventByIdAsync(eventId);
		
		if (eventData == null)
		{
			return;
		}

		await _participationService.DeleteParticipationsByEventIdAsync(eventId);
		await _commentService.DeleteCommentsAsync(eventData.Comments);
		await _eventStore.DeleteAsync(eventId);
	}

	public async Task<(bool success, string? error)> AddCommentAsync(string eventId, string senderId, string content, string? repliedMessageId = null)
	{
		var eventData = await GetEventByIdAsync(eventId);
		
		if (eventData == null)
		{
			return (false, "Event not found.");
		}

		var comment = new Comment
		{
			Id = Guid.NewGuid().ToString(),
			SenderId = senderId,
			Content = content,
			TimeStamp = DateTime.Now,
			RepliedMessageId = repliedMessageId
		};

		try
		{
			await _commentService.CreateCommentAsync(comment);
			eventData.Comments.Add(comment.Id);
			await _eventStore.UpdateAsync(eventId, eventData);
			
			return (true, null);
		}
		catch (Exception exception)
		{
			return (false, exception.Message);
		}
	}

	public async Task<EventViewModel?> GetEventViewModelAsync(string eventId, string currentUserId)
	{
		var eventData = await GetEventByIdAsync(eventId);
		
		if (eventData == null)
		{
			return null;
		}

		var owner = _userService.GetUserById(eventData.OwnerId);
		
		if (owner == null)
		{
			return null;
		}

		var participations = await _participationService.GetParticipationsByEventIdAsync(eventData.Id);
		var userParticipation = participations.FirstOrDefault(participation => participation.UserId == currentUserId);

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
			Category = eventData.Category,
			Status = eventData.Status,
			Owner = owner.ToSummaryViewModel(),
			Participants = MapParticipants(participations),
			Comments = await MapCommentsAsync(eventData.Comments),
			IsOwner = eventData.OwnerId == currentUserId,
			IsJoined = userParticipation?.Status == "Accepted",
			UserStatus = userParticipation?.Status
		};
	}

	private List<ParticipationViewModel> MapParticipants(List<Participation> participations)
	{
		return participations
			.Select(participation =>
			{
				var user = _userService.GetUserById(participation.UserId);
				return user != null ? participation.ToViewModel(user.ToSummaryViewModel()) : null;
			})
			.OfType<ParticipationViewModel>()
			.ToList();
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

			var sender = _userService.GetUserById(comment.SenderId);
			return comment.ToViewModel(sender?.ToSummaryViewModel()!);
		});

		var results = await Task.WhenAll(tasks);
		
		return results
			.OfType<CommentViewModel>()
			.ToList();
	}
}
