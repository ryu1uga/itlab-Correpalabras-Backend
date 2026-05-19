namespace CorrePalabras.DTOs.Common
{
    public class PageDTO
    {
        public Guid Id { get; set; }
        public Guid StoryId { get; set; }
        public int PageOrder { get; set; } = 0;
        public string ImageUrl { get; set; } = "";
    }
}