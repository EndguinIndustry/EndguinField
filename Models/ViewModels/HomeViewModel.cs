using EndguinField.Models;
public class HomeViewModel
{
    public List<EventCardModel> PendingEvents { get; set; } = new List<EventCardModel>();
    public List<EventCardModel> ParticipatingEvents { get; set; } = new List<EventCardModel>();
    public List<EventCardModel> OpenEvents { get; set; } = new List<EventCardModel>();
    public List<EventCardModel> MyOngoingEvents { get; set; } = new List<EventCardModel>();
}