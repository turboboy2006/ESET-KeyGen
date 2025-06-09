using System;
using System.ComponentModel.DataAnnotations;

namespace RankTracker.Core.Models
{
    public class RankingLog
    {
        public int Id { get; set; }

        public int WebsiteId { get; set; }
        public virtual Website Website { get; set; }

        public int KeywordId { get; set; }
        public virtual Keyword Keyword { get; set; }

        public int? Rank { get; set; } // Nullable if not found

        [Required]
        public DateTime SearchDate { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }
    }
}
