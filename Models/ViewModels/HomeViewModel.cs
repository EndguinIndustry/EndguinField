using EndguinField.Models;
public class HomeViewModel
{
    public List<EventCardModel> JoinedEvents { get; set; } = new List<EventCardModel>();
    public List<EventCardModel> OpenEvents { get; set; } = new List<EventCardModel>();
}