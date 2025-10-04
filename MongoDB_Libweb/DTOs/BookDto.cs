using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDB_Libweb.DTOs
{
    [BsonIgnoreExtraElements]
    public class BookDto
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("title")]
        public string Title { get; set; } = null!;

        [BsonElement("authors")]
        public List<string> Authors { get; set; } = new List<string>(); // Adjusted for multi Id - Contains author IDs for processing

        [BsonElement("categories")]
        public List<string> Categories { get; set; } = new List<string>(); // Adjusted for multi Id - Contains category IDs for processing

        [BsonElement("description")]
        public string Description { get; set; } = null!;

        [BsonElement("publishYear")]
        public int? PublishYear { get; set; }

        [BsonElement("fileUrl")]
        public string? FileUrl { get; set; }

        [BsonElement("fileFormat")]
        public string? FileFormat { get; set; }

        [BsonElement("isAvailable")]
        public bool IsAvailable { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }

    // Adjusted for multi Id - DTO for displaying book details with author/category names
    public class BookDisplayDto
    {
        public string Id { get; set; } = null!;
        public string Title { get; set; } = null!;
        public List<string> Authors { get; set; } = new List<string>(); // Author names for display
        public List<string> Categories { get; set; } = new List<string>(); // Category names for display
        public string Description { get; set; } = null!;
        public int? PublishYear { get; set; }
        public string? FileUrl { get; set; }
        public string? FileFormat { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class BookCreateDto
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "At least one author is required")]
        public List<string> AuthorsIds { get; set; } = new List<string>(); // Adjusted for multi Id - JSON string array

        [Required(ErrorMessage = "At least one category is required")]
        public List<string> CategoriesIds { get; set; } = new List<string>(); // Adjusted for multi Id - JSON string array

        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; } = null!;

        [Range(1900, 2100, ErrorMessage = "Publish year must be between 1900 and 2100")]
        public int? PublishYear { get; set; }

        [Required(ErrorMessage = "File is required")]
        public IFormFile File { get; set; } = null!;
    }

    public class BookUpdateDto
    {
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string? Title { get; set; }

        public List<string>? AuthorsIds { get; set; } // Adjusted for multi Id - JSON string array

        public List<string>? CategoriesIds { get; set; } // Adjusted for multi Id - JSON string array

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        [Range(1900, 2100, ErrorMessage = "Publish year must be between 1900 and 2100")]
        public int? PublishYear { get; set; }

        public IFormFile? File { get; set; }

        public bool? IsAvailable { get; set; }

        public bool ReplaceFile { get; set; } = false;
    }

    public class BookSearchDto
    {
        public string? SearchQuery { get; set; }
        public List<string>? Categories { get; set; }
        public List<string>? Authors { get; set; }
        public int? MinYear { get; set; }
        public int? MaxYear { get; set; }
        public bool? IsAvailable { get; set; }
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 10;
    }

    public class BookStatusUpdateDto
    {
        [Required]
        public bool IsAvailable { get; set; }
    }
}
