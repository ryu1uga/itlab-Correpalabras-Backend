using System.ComponentModel.DataAnnotations.Schema;

namespace CorrePalabras.Models.Common
{
    [Table("Avatar")] 
    public class Avatar
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid StoryId { get; set; }
        public string AvatarUrl { get; set; } = "";

        public Story? Story { get; set; }

        public ICollection<UnlockedAvatar>? UnlockedAvatars { get; set; }
    }
}