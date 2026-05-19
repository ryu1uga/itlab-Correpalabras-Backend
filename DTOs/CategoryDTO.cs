namespace CorrePalabras.DTOs.Common
{
    public class CategoryDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string Code { get; set; } = "";
        public DateTime UpdatedAt { get; set; }
        public int CategoryOrder { get; set; } = 0;
    }
}