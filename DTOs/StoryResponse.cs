namespace CorrePalabras.DTOs
{
    public class StorySummaryResponse
    {
        /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
        public Guid Id { get; set; }
        /// <example>El dragón y la luna</example>
        public string Title { get; set; } = "";
        /// <example>/api/stories/3fa85f64/image</example>
        public string Thumbnail { get; set; } = "";
        public IEnumerable<object> StoryCategories { get; set; } = new List<object>();
        /// <example>320</example>
        public int TotalWords { get; set; }
    }

    public class PageContentResponse
    {
        public Guid Id { get; set; }
        public Guid PageId { get; set; }
        public Guid LanguageId { get; set; }
        /// <example>45</example>
        public int CountWords { get; set; }
        /// <example>Érase una vez...</example>
        public string Content { get; set; } = "";
    }

    public class PageResponse
    {
        public Guid Id { get; set; }
        /// <example>1</example>
        public int PageOrder { get; set; }
        /// <example>/api/pages/3fa85f64/image</example>
        public string ImageUrl { get; set; } = "";
        public IEnumerable<PageContentResponse> PageContents { get; set; } = new List<PageContentResponse>();
    }

    public class AttachmentResponse
    {
        public Guid Id { get; set; }
        /// <example>/api/attachments/3fa85f64/image</example>
        public string ImageUrl { get; set; } = "";
        /// <example>decoracion</example>
        public string TypeImage { get; set; } = "";
        /// <example>top-left</example>
        public string Position { get; set; } = "";
        /// <example>1</example>
        public int OrderAttachments { get; set; }
        public Guid LanguageId { get; set; }
    }

    public class StoryDetailResponse
    {
        public Guid Id { get; set; }
        /// <example>María García</example>
        public string Author { get; set; } = "";
        /// <example>Carlos López</example>
        public string Illustrator { get; set; } = "";
        /// <example>El dragón y la luna</example>
        public string Title { get; set; } = "";
        /// <example>10</example>
        public int CountPages { get; set; }
        /// <example>/api/stories/3fa85f64/image</example>
        public string Thumbnail { get; set; } = "";
        public DateTime UpdatedAt { get; set; }
        public IEnumerable<object> StoryCategories { get; set; } = new List<object>();
        public IEnumerable<object> StoryLanguages { get; set; } = new List<object>();
        public IEnumerable<PageResponse> Pages { get; set; } = new List<PageResponse>();
        public IEnumerable<AttachmentResponse> Attachments { get; set; } = new List<AttachmentResponse>();
    }

    public class MostReadResponse
    {
        public Guid StoryId { get; set; }
        /// <example>El dragón y la luna</example>
        public string Title { get; set; } = "";
        /// <example>152</example>
        public int Reads { get; set; }
    }
}
