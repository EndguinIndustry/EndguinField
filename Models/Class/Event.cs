namespace EndguinField.Models
{
    public class Event
    {
        public string EventId { get; set; } = string.Empty;
        public string OwnerId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Images { get; set; } = new List<string>();
        public List<Enroll> Enrolls { get; set; } = new List<Enroll>();
        public string Location { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public int MaxEnrolls { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<User> BlackLists { get; set; } = new List<User>();
        public List<string> Messages { get; set; } = new List<string>();
    }
}