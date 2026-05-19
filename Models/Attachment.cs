using System.ComponentModel.DataAnnotations.Schema;

namespace CorrePalabras.Models.Common
{
    [Table("Attachment")] 
    public class Attachment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid StoryId { get; set; }
        public Guid LanguageId { get; set; }
        public string ImageUrl { get; set; } = "";
        public string TypeImage { get; set; } = "";
        public string Position { get; set; } = "";
        public int OrderAttachments { get; set; } = 0;

        public Story? Story { get; set; }
        public Language? Language { get; set; }
    }
}