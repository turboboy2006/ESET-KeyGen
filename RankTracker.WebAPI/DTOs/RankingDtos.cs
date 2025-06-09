using System;
using System.ComponentModel.DataAnnotations;

namespace RankTracker.WebAPI.DTOs
{
    public class ManualRankCheckRequestDto
    {
        [Required]
        public int WebsiteId { get; set; }

        [Required]
        public int KeywordId { get; set; }
    }

    public class RankingLogDto
    {
        public int Id { get; set; }
        public int WebsiteId { get; set; }
        public string WebsiteName { get; set; }
        public string WebsiteUrl { get; set; }
        public int KeywordId { get; set; }
        public string KeywordText { get; set; }
        public int? Rank { get; set; }
        public DateTime SearchDate { get; set; }
        public string Notes { get; set; }
    }

    public class RankingHistoryQueryDto
    {
        public int? WebsiteId { get; set; }
        public int? KeywordId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
}
