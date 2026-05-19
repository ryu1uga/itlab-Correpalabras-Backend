using System.ComponentModel.DataAnnotations.Schema;

namespace CorrePalabras.Models.Common
{
    [Table("Category")] 
    public class Category
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "";
        public string Code { get; set; } = "";
        public DateTime UpdatedAt { get; set; }
        public int CategoryOrder { get; set; } = 0;
        public int Counter { get; set; } = 0;

        public ICollection<StoryCategory>? StoryCategories { get; set; }
    }
}