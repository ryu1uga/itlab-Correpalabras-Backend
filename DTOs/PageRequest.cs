namespace CorrePalabras.DTOs
{
    public class PageRequest
    {
        public Guid Id { get; set; }
        public Guid StoryId { get; set; }
        public int PageOrder { get; set; } = 0;
        public string ImageUrl { get; set; } = "";
    }
}
