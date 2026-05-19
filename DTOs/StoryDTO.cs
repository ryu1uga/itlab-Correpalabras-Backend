namespace CorrePalabras.DTOs.Common
{
    public class StoryDTO
    {
        public Guid Id { get; set; }
        public string Author { get; set; } = "";
        public string Illustrator { get; set; } = "";
        public string Title { get; set; } = "";
        public int CountPages { get; set; } = 0;
        public string Thumbnail { get; set; } = "";
        public DateTime UpdatedAt { get; set; }
    }
}