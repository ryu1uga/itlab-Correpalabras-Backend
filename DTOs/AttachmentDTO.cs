namespace CorrePalabras.DTOs.Common
{
    public class AttachmentDTO
    {
        public Guid Id { get; set; }
        public Guid StoryId { get; set; }
        public Guid LanguageId { get; set; }
        public string ImageUrl { get; set; } = "";
        public string TypeImage { get; set; } = "";
        public string Position { get; set; } = "";
        public int OrderAttachments { get; set; } = 0;
    }
}