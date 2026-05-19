using System.ComponentModel.DataAnnotations.Schema;

namespace CorrePalabras.Models.Common
{
    [Table("Badge")]
    public class Badge
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "";
        public string BadgeUrl { get; set; } = "";

        public ICollection<UnlockedBadge>? UnlockedBadges { get; set; }
    }
}