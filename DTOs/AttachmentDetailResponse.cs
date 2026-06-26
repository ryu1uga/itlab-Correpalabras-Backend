namespace CorrePalabras.DTOs
{
    public class AttachmentDetailResponse
    {
        public Guid Id { get; set; }
        public Guid StoryId { get; set; }
        /// <example>/api/attachments/3fa85f64/image</example>
        public string ImageUrl { get; set; } = "";
        public string TypeImage { get; set; } = "";
        public string Position { get; set; } = "";
        public int OrderAttachments { get; set; }
        public Guid LanguageId { get; set; }
    }
}
