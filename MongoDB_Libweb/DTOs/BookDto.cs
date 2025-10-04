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
        public List<string> Authors { get; set; } = new List<string>();

        [BsonElement("categories")]
        public List<string> Categories { get; set; } = new List<string>();

        [BsonElement("description")]
        public string Description { get; set; } = null!;

        [BsonElement("publishYear")]
        public int PublishYear { get; set; }

        [BsonElement("fileUrl")]
        public string FileUrl { get; set; } = null!;

        [BsonElement("fileFormat")]
        public string FileFormat { get; set; } = null!;

        [BsonElement("isAvailable")]
        public bool IsAvailable { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }

    public class BookCreateDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = null!;

        [Required]
        public List<string> Authors { get; set; } = new List<string>();

        [Required]
        public List<string> Categories { get; set; } = new List<string>();

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = null!;

        [Required]
        public int PublishYear { get; set; }

        [Required]
        public string FileUrl { get; set; } = null!;

        [Required]
        public string FileFormat { get; set; } = null!;
    }

    public class BookUpdateDto
    {
        [StringLength(200)]
        public string? Title { get; set; }

        public List<string>? Authors { get; set; }

        public List<string>? Categories { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public int? PublishYear { get; set; }

        public string? FileUrl { get; set; }

        public string? FileFormat { get; set; }

        public bool? IsAvailable { get; set; }
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
}
