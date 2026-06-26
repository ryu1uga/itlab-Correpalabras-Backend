namespace CorrePalabras.DTOs
{
    public class LanguageResponse
    {
        public Guid Id { get; set; }
        /// <example>Español</example>
        public string Name { get; set; } = "";
        /// <example>ES</example>
        public string Code { get; set; } = "";
    }

    public class MostDemandedLanguageResponse
    {
        public Guid LanguageId { get; set; }
        /// <example>Español</example>
        public string Name { get; set; } = "";
        /// <example>210</example>
        public int Demands { get; set; }
    }
}
