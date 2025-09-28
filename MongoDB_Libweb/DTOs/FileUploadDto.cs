using System.ComponentModel.DataAnnotations;

namespace MongoDB_Libweb.DTOs
{
    public class FileUploadDto
    {
        public string Id { get; set; } = null!;
        public string Filename { get; set; } = null!;
        public long Length { get; set; }
        public DateTime UploadDate { get; set; }
        public string BookId { get; set; } = null!;
    }

    public class FileUploadCreateDto
    {
        [Required]
        public string Filename { get; set; } = null!;

        [Required]
        public long Length { get; set; }

        [Required]
        public string BookId { get; set; } = null!;
    }
}
