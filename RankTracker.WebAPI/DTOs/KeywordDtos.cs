using System.ComponentModel.DataAnnotations;

namespace RankTracker.WebAPI.DTOs
{
    public class CreateKeywordDto
    {
        [Required]
        [StringLength(255)]
        public string Text { get; set; }
    }

    public class KeywordDto
    {
        public int Id { get; set; }
        public string Text { get; set; }
    }
}
