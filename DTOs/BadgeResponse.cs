namespace CorrePalabras.DTOs
{
    public class BadgeResponse
    {
        public Guid Id { get; set; }
        /// <example>Lector Estrella</example>
        public string Name { get; set; } = "";
        /// <example>Lee 10 cuentos</example>
        public string Description { get; set; } = "";
    }
}
