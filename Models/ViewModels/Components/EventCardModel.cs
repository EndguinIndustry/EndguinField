namespace EndguinField.Models
{
    public class EventCardModel
    {
        public string Name { get; set; } = string.Empty;
        public string ImgSrc { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Location { get; set; } = string.Empty;
        public int ParticipantCount { get; set; }
        public int MaxParticipants { get; set; }
    }
}