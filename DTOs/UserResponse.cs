namespace CorrePalabras.DTOs
{
    public class ProfileSummaryResponse
    {
        public Guid Id { get; set; }
        public Guid AvatarId { get; set; }
        /// <example>/api/avatars/3fa85f64/image</example>
        public string AvatarUrl { get; set; } = "";
        /// <example>dragoncito22</example>
        public string Username { get; set; } = "";
        /// <example>M</example>
        public string Gender { get; set; } = "";
        public DateTime BirthDate { get; set; }
    }

    public class UserResponse
    {
        public Guid Id { get; set; }
        /// <example>Juan Pérez</example>
        public string Name { get; set; } = "";
        /// <example>juan@example.com</example>
        public string Email { get; set; } = "";
        /// <example>0</example>
        public int UserType { get; set; }
        public IEnumerable<ProfileSummaryResponse> Profiles { get; set; } = new List<ProfileSummaryResponse>();
    }
}
