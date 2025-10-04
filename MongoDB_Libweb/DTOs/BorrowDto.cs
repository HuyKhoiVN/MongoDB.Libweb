using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDB_Libweb.DTOs
{
    public class BorrowDto
    {
        public string Id { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string BookId { get; set; } = null!;
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; } = null!;
    }

    [BsonIgnoreExtraElements]
    public class BorrowDetailDto
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
        public string Status { get; set; } = null!;

        [BsonElement("user")]
        public UserDto User { get; set; } = null!;

        [BsonElement("book")]
        public BookDto Book { get; set; } = null!;
    }

    public class BorrowCreateDto
    {
        [Required]
        public string UserId { get; set; } = null!;

        [Required]
        public string BookId { get; set; } = null!;

        [Required]
        public DateTime DueDate { get; set; }
    }

    public class BorrowReturnDto
    {
        [Required]
        public string BorrowId { get; set; } = null!;
    }

    public class BorrowSearchDto
    {
        public string? UserId { get; set; }
        public string? BookId { get; set; }
        public string? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 10;
    }
}
