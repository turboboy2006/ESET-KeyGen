using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RankTracker.Core.Models
{
    public class Website
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Required]
        [StringLength(2048)]
        [Url]
        public string Url { get; set; }

        public virtual ICollection<WebsiteKeyword> WebsiteKeywords { get; set; } = new List<WebsiteKeyword>();
    }
}
