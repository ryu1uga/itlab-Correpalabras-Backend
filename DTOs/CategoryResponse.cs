namespace CorrePalabras.DTOs
{
    public class CategoryResponse
    {
        public Guid Id { get; set; }
        /// <example>Aventura</example>
        public string Name { get; set; } = "";
        /// <example>ADV</example>
        public string Code { get; set; } = "";
        /// <example>true</example>
        public bool IsVisible { get; set; }
    }

    public class MostVisitedCategoryResponse
    {
        public Guid CategoryId { get; set; }
        /// <example>Aventura</example>
        public string Name { get; set; } = "";
        /// <example>87</example>
        public int Visits { get; set; }
    }
}
