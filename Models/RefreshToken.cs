using System.ComponentModel.DataAnnotations.Schema;

namespace CorrePalabras.Models.Common
{
    [Table("RefreshToken")]
    public class RefreshToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string Token { get; set; } = "";
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RevokedAt { get; set; }
        public string? ReplacedByToken { get; set; }

        public User? User { get; set; }

        [NotMapped]
        public bool IsActive => RevokedAt == null && DateTime.UtcNow < ExpiresAt;
    }
}
