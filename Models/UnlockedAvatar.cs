using System.ComponentModel.DataAnnotations.Schema;

namespace CorrePalabras.Models.Common
{
    [Table("UnlockedAvatar")] 
    public class UnlockedAvatar
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ProfileId { get; set; }
        public Guid AvatarId { get; set; }

        public Profile? Profile { get; set; }
        public Avatar? Avatar { get; set; }
    }
}