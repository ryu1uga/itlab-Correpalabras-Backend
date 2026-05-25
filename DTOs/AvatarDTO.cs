namespace CorrePalabras.DTOs.Common
{
    public class AvatarDTO
    {
        public Guid Id { get; set; }
        public Guid? StoryId { get; set; }
        public string AvatarUrl { get; set; } = "";
    }
}