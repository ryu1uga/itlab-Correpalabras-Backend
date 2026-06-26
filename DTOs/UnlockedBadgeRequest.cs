namespace CorrePalabras.DTOs
{
    public class UnlockedBadgeRequest
    {
        public Guid Id { get; set; }
        public Guid ProfileId { get; set; }
        public Guid BadgeId { get; set; }
    }
}
