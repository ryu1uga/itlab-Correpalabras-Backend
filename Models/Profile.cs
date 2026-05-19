using System.ComponentModel.DataAnnotations.Schema;

namespace CorrePalabras.Models.Common
{
    [Table("Profile")] 
    public class Profile
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid AvatarId { get; set; }
        public string Username { get; set; } = "";
        public string Gender { get; set; } = "";
        public DateTime BirthDate { get; set; }
        public Guid UserId { get; set; }

        public User? User { get; set; }
        public Avatar? Avatar { get; set; }

        public ICollection<UnlockedBadge>? UnlockedBadges { get; set; }
        public ICollection<UnlockedAvatar>? UnlockedAvatars { get; set; }
        public ICollection<ProfileStory>? ProfileStories { get; set; }
    }
}