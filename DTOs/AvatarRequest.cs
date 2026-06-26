namespace CorrePalabras.DTOs
{
    public class AvatarRequest
    {
        public Guid Id { get; set; }
        public Guid? StoryId { get; set; }
        public string AvatarUrl { get; set; } = "";
    }
}
