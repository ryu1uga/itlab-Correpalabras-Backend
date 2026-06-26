namespace CorrePalabras.DTOs
{
    public class ProfileResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid AvatarId { get; set; }
        /// <example>/api/avatars/3fa85f64/image</example>
        public string AvatarUrl { get; set; } = "";
        /// <example>dragoncito22</example>
        public string Username { get; set; } = "";
        /// <example>M</example>
        public string Gender { get; set; } = "";
        public DateTime BirthDate { get; set; }
    }

    public class ProfileCountResponse
    {
        /// <example>350</example>
        public int Total { get; set; }
    }

    public class ProfileGenderCountResponse
    {
        /// <example>180</example>
        public int Male { get; set; }
        /// <example>170</example>
        public int Female { get; set; }
    }
}
