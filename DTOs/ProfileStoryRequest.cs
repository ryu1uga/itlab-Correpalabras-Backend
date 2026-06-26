namespace CorrePalabras.DTOs
{
    public class ProfileStoryRequest
    {
        public Guid Id { get; set; }
        public Guid StoryLanguageId { get; set; }
        public Guid ProfileId { get; set; }
        public bool IsDownloaded { get; set; } = false;
        public bool IsRead { get; set; } = false;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
