using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDB_Libweb.Models
{
    public class Book
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
        public int? PublishYear { get; set; }

        [BsonElement("fileUrl")]
        public string? FileUrl { get; set; }

        [BsonElement("fileFormat")]
        public string? FileFormat { get; set; } // "PDF", "EPUB"

        [BsonElement("isAvailable")]
        public bool IsAvailable { get; set; } = true;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }
}
