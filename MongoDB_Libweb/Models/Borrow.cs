using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDB_Libweb.Models
{
    public class Borrow
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = null!;

        [BsonElement("bookId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string BookId { get; set; } = null!;

        [BsonElement("borrowDate")]
        public DateTime BorrowDate { get; set; }

        [BsonElement("dueDate")]
        public DateTime DueDate { get; set; }

        [BsonElement("returnDate")]
        public DateTime? ReturnDate { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } = null!; // "Borrowed", "Returned", "Overdue"
    }
}
