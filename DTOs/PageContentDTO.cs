namespace CorrePalabras.DTOs.Common
{
    public class PageContentDTO
    {
        public Guid Id { get; set; }
        public Guid PageId { get; set; }
        public Guid LanguageId { get; set; }
        public int CountWords { get; set; } = 0;
        public string Content { get; set; } = "";
    }
}