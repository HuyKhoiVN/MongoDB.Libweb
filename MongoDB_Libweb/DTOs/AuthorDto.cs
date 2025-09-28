using System.ComponentModel.DataAnnotations;

namespace MongoDB_Libweb.DTOs
{
    public class AuthorDto
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Bio { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

    public class AuthorCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [StringLength(1000)]
        public string Bio { get; set; } = null!;
    }

    public class AuthorUpdateDto
    {
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(1000)]
        public string? Bio { get; set; }
    }
}
