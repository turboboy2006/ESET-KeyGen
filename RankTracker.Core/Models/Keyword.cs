using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RankTracker.Core.Models
{
    public class Keyword
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Text { get; set; }

        public virtual ICollection<WebsiteKeyword> WebsiteKeywords { get; set; } = new List<WebsiteKeyword>();
    }
}
