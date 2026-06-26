namespace CorrePalabras.DTOs.Common
{
    public class LoginResponseDTO
    {
        /// <example>59cb971a-04fd-45df-944e-47640c261ac9</example>
        public Guid Id { get; set; }
        /// <example>Juan Pérez</example>
        public string Name { get; set; } = "";
        /// <example>usuario@example.com</example>
        public string Email { get; set; } = "";
        /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ...</example>
        public string Token { get; set; } = "";
        /// <example>QHPWZAHDSRhWzoh/FMMSnpSl1TEXelSfg6qTDt2mFYxZ7LKEfaNkjwTT==</example>
        public string RefreshToken { get; set; } = "";
    }
}
