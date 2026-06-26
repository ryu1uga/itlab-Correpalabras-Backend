namespace CorrePalabras.DTOs
{
    public class BadgeRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string BadgeUrl { get; set; } = "";
    }
}
