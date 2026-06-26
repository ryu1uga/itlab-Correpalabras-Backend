namespace CorrePalabras.DTOs
{
    public class AvatarResponse
    {
        public Guid Id { get; set; }
        /// <example>/api/avatars/3fa85f64/image</example>
        public string AvatarUrl { get; set; } = "";
        public Guid? StoryId { get; set; }
    }
}
