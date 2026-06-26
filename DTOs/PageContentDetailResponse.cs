namespace CorrePalabras.DTOs
{
    public class PageContentDetailResponse
    {
        public Guid Id { get; set; }
        public Guid PageId { get; set; }
        public Guid LanguageId { get; set; }
        public int CountWords { get; set; }
        public string Content { get; set; } = "";
    }
}
