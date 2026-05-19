namespace CorrePalabras.DTOs.Common
{
    public class ProfileDTO
    {
        public Guid AvatarId { get; set; }
        public string Username { get; set; } = "";
        public string Gender { get; set; } = "";
        public DateTime BirthDate { get; set; }
    }
}