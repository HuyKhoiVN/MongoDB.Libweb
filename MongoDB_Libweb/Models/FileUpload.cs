using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDB_Libweb.Models
{
    public class FileUpload
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("filename")]
        public string Filename { get; set; } = null!;

        [BsonElement("length")]
        public long Length { get; set; }

        [BsonElement("uploadDate")]
        public DateTime UploadDate { get; set; }

        [BsonElement("bookId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string BookId { get; set; } = null!;
    }
}
