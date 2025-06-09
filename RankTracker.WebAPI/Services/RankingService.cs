using Microsoft.EntityFrameworkCore;
using RankTracker.Core.Data;
using RankTracker.Core.Models;
using RankTracker.WebAPI.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RankTracker.WebAPI.Services
{
    public class RankingService : IRankingService
    {
        private readonly RankTrackerDbContext _context;
        // private readonly ISeleniumRankChecker _seleniumRankChecker; // For Step 3

        public RankingService(RankTrackerDbContext context) // ISeleniumRankChecker will be added later
        {
            _context = context;
        }

        public async Task<RankingLogDto> PerformManualRankCheckAsync(ManualRankCheckRequestDto request)
        {
            var website = await _context.Websites.FindAsync(request.WebsiteId);
            var keyword = await _context.Keywords.FindAsync(request.KeywordId);

            if (website == null || keyword == null)
            {
                throw new ArgumentException("Website or Keyword not found.");
            }

            // Placeholder for Selenium Rank Checking (Step 3)
            int? rank = new Random().Next(1, 101);
            if (rank > 50) rank = null;
            string notes = "Manual check (simulated result).";
            // End Placeholder

            var rankingLog = new RankingLog
            {
                WebsiteId = request.WebsiteId,
                KeywordId = request.KeywordId,
                Rank = rank,
                SearchDate = DateTime.UtcNow,
                Notes = notes
            };

            _context.RankingLogs.Add(rankingLog);
            await _context.SaveChangesAsync();

            return new RankingLogDto
            {
                Id = rankingLog.Id,
                WebsiteId = website.Id,
                WebsiteName = website.Name,
                WebsiteUrl = website.Url,
                KeywordId = keyword.Id,
                KeywordText = keyword.Text,
                Rank = rankingLog.Rank,
                SearchDate = rankingLog.SearchDate,
                Notes = rankingLog.Notes
            };
        }

        public async Task<IEnumerable<RankingLogDto>> GetRankingHistoryAsync(RankingHistoryQueryDto query)
        {
            var queryable = _context.RankingLogs
                                .Include(rl => rl.Website)
                                .Include(rl => rl.Keyword)
                                .AsQueryable();

            if (query.WebsiteId.HasValue)
            {
                queryable = queryable.Where(rl => rl.WebsiteId == query.WebsiteId.Value);
            }
            if (query.KeywordId.HasValue)
            {
                queryable = queryable.Where(rl => rl.KeywordId == query.KeywordId.Value);
            }
            if (query.DateFrom.HasValue)
            {
                queryable = queryable.Where(rl => rl.SearchDate >= query.DateFrom.Value);
            }
            if (query.DateTo.HasValue)
            {
                queryable = queryable.Where(rl => rl.SearchDate < query.DateTo.Value.AddDays(1));
            }

            return await queryable
                .OrderByDescending(rl => rl.SearchDate)
                .Select(rl => new RankingLogDto
                {
                    Id = rl.Id,
                    WebsiteId = rl.WebsiteId,
                    WebsiteName = rl.Website.Name,
                    WebsiteUrl = rl.Website.Url,
                    KeywordId = rl.KeywordId,
                    KeywordText = rl.Keyword.Text,
                    Rank = rl.Rank,
                    SearchDate = rl.SearchDate,
                    Notes = rl.Notes
                })
                .ToListAsync();
        }
    }
}
