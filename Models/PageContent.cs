using System.ComponentModel.DataAnnotations.Schema;

namespace CorrePalabras.Models.Common
{
    [Table("PageContent")]
    public class PageContent
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid PageId { get; set; }
        public Guid LanguageId { get; set; }
        public int CountWords { get; set; } = 0;
        public string Content { get; set; } = "";

        public Page? Page { get; set; }
        public Language? Language { get; set; }
    }
}