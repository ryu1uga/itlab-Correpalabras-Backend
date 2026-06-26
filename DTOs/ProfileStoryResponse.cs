namespace CorrePalabras.DTOs
{
    public class ProfileStoryResponse
    {
        public Guid Id { get; set; }
        public Guid ProfileId { get; set; }
        public Guid StoryLanguageId { get; set; }
        /// <example>false</example>
        public bool IsDownloaded { get; set; }
        /// <example>false</example>
        public bool IsRead { get; set; }
    }
}
