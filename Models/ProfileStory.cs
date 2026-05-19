using System.ComponentModel.DataAnnotations.Schema;

namespace CorrePalabras.Models.Common
{
    [Table("ProfileStory")] 
    public class ProfileStory
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid StoryLanguageId { get; set; }
        public Guid ProfileId { get; set; }
        public bool IsDownloaded { get; set; } = false;
        public bool IsRead { get; set; } = false;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public Profile? Profile { get; set; }
        public StoryLanguage? StoryLanguage { get; set; }
    }
}