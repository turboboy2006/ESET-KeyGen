using System.Collections.Generic; // Required if you were to add collections back here

namespace RankTracker.Core.Models
{
    public class WebsiteKeyword
    {
        public int WebsiteId { get; set; }
        public virtual Website Website { get; set; }

        public int KeywordId { get; set; }
        public virtual Keyword Keyword { get; set; }
    }
}
