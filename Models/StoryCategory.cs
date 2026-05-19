using System.ComponentModel.DataAnnotations.Schema;

namespace CorrePalabras.Models.Common
{
    [Table("StoryCategory")] 
    public class StoryCategory
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid StoryId { get; set; }
        public Guid CategoryId { get; set; }

        public Story? Story { get; set; }
        public Category? Category { get; set; }
    }
}