using System.ComponentModel.DataAnnotations;

namespace RankTracker.WebAPI.DTOs
{
    public class CreateWebsiteDto
    {
        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Required]
        [StringLength(2048)]
        [Url]
        public string Url { get; set; }
    }

    public class WebsiteDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
    }

    public class UpdateWebsiteDto
    {
        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Required]
        [StringLength(2048)]
        [Url]
        public string Url { get; set; }
    }
}
