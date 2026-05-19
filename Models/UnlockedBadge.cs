using System.ComponentModel.DataAnnotations.Schema;

namespace CorrePalabras.Models.Common
{
    [Table("UnlockedBadge")]
    public class UnlockedBadge
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ProfileId { get; set; }
        public Guid BadgeId { get; set; }

        public Profile? Profile { get; set; }
        public Badge? Badge { get; set; }
    }
}