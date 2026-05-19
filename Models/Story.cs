using System.ComponentModel.DataAnnotations.Schema;

namespace CorrePalabras.Models.Common
{
    [Table("Story")] 
    public class Story
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Author { get; set; } = "";
        public string Illustrator { get; set; } = "";
        public string Title { get; set; } = "";
        public int CountPages { get; set; } = 0;
        public string Thumbnail { get; set; } = "";
        public DateTime UpdatedAt { get; set; }
        public int Counter { get; set; } = 0;

        public ICollection<StoryCategory>? StoryCategories { get; set; }
        public ICollection<StoryLanguage>? StoryLanguages { get; set; }
        public ICollection<Page>? Pages { get; set; }
        public ICollection<Attachment>? Attachments { get; set; }
        public ICollection<Avatar>? Avatars { get; set; }
    }
}