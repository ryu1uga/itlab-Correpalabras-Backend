using System.ComponentModel.DataAnnotations.Schema;

namespace CorrePalabras.Models.Common
{
    [Table("Language")] 
    public class Language
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "";
        public int Counter { get; set; } = 0;

        public ICollection<StoryLanguage>? StoryLanguages { get; set; }
        public ICollection<Attachment>? Attachments { get; set; }
        public ICollection<PageContent>? PageContents { get; set; }
    }
} 