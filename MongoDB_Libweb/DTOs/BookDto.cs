using System.ComponentModel.DataAnnotations;

namespace MongoDB_Libweb.DTOs
{
    public class BookDto
    {
        public string Id { get; set; } = null!;
        public string Title { get; set; } = null!;
        public List<string> Authors { get; set; } = new List<string>();
        public List<string> Categories { get; set; } = new List<string>();
        public string Description { get; set; } = null!;
        public int PublishYear { get; set; }
        public string FileUrl { get; set; } = null!;
        public string FileFormat { get; set; } = null!;
        public bool IsAvailable { get; set; }
        public DateTime CreatedAt { get; set; }
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
        [Range(1000, 2025)]
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

        [Range(1000, 2025)]
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
