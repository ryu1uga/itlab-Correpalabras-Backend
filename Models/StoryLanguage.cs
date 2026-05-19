using System.ComponentModel.DataAnnotations.Schema;

namespace CorrePalabras.Models.Common
{
    [Table("StoryLanguage")] 
    public class StoryLanguage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid StoryId { get; set; }
        public Guid LanguageId { get; set; }

        public Story? Story { get; set; }
        public Language? Language { get; set; }

        public ICollection<ProfileStory>? ProfileStories { get; set; }
    }
}