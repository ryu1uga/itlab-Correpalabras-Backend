namespace CorrePalabras.DTOs
{
    public class PageDetailResponse
    {
        public Guid Id { get; set; }
        public Guid StoryId { get; set; }
        public int PageOrder { get; set; }
        /// <example>/api/pages/3fa85f64/image</example>
        public string ImageUrl { get; set; } = "";
    }
}
