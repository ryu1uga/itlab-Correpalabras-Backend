using System.ComponentModel.DataAnnotations.Schema;

namespace CorrePalabras.Models.Common
{
    [Table("Page")]
    public class Page
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid StoryId { get; set; }
        public int PageOrder { get; set; } = 0;
        public string ImageUrl { get; set; } = "";

        public Story? Story { get; set; }
        public ICollection<PageContent>? PageContents { get; set; }
    }
}