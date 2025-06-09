using Microsoft.EntityFrameworkCore;
using RankTracker.Core.Data;
using RankTracker.Core.Models;
using RankTracker.WebAPI.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RankTracker.WebAPI.Services
{
    public class KeywordService : IKeywordService
    {
        private readonly RankTrackerDbContext _context;

        public KeywordService(RankTrackerDbContext context)
        {
            _context = context;
        }

        public async Task<KeywordDto> CreateKeywordAsync(CreateKeywordDto createKeywordDto)
        {
            var existingKeyword = await _context.Keywords.FirstOrDefaultAsync(k => k.Text == createKeywordDto.Text);
            if (existingKeyword != null)
            {
                return new KeywordDto { Id = existingKeyword.Id, Text = existingKeyword.Text };
            }

            var keyword = new Keyword { Text = createKeywordDto.Text };
            _context.Keywords.Add(keyword);
            await _context.SaveChangesAsync();
            return new KeywordDto { Id = keyword.Id, Text = keyword.Text };
        }

        public async Task<IEnumerable<KeywordDto>> GetAllKeywordsAsync()
        {
            return await _context.Keywords
                .Select(k => new KeywordDto { Id = k.Id, Text = k.Text })
                .ToListAsync();
        }

        public async Task<KeywordDto> GetKeywordByIdAsync(int id)
        {
            var keyword = await _context.Keywords.FindAsync(id);
            if (keyword == null) return null;
            return new KeywordDto { Id = keyword.Id, Text = keyword.Text };
        }

        public async Task<bool> DeleteKeywordAsync(int id)
        {
            var keyword = await _context.Keywords.Include(k => k.WebsiteKeywords)
                                                 .FirstOrDefaultAsync(k => k.Id == id);
            if (keyword == null) return false;

            var rankingLogs = await _context.RankingLogs.Where(rl => rl.KeywordId == id).ToListAsync();
            if (rankingLogs.Any()) _context.RankingLogs.RemoveRange(rankingLogs);

            // WebsiteKeyword entries linked to this keyword are removed by cascade delete configured in DbContext
            // or should be removed manually if not.
            // If EF Core is configured for cascade on WebsiteKeyword from Keyword, this is fine.
            // Otherwise: _context.WebsiteKeywords.RemoveRange(keyword.WebsiteKeywords);

            _context.Keywords.Remove(keyword);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssociateKeywordWithWebsiteAsync(int websiteId, int keywordId)
        {
            var websiteExists = await _context.Websites.AnyAsync(w => w.Id == websiteId);
            var keywordExists = await _context.Keywords.AnyAsync(k => k.Id == keywordId);

            if (!websiteExists || !keywordExists) return false;

            var existingAssociation = await _context.WebsiteKeywords
                .FirstOrDefaultAsync(wk => wk.WebsiteId == websiteId && wk.KeywordId == keywordId);

            if (existingAssociation != null) return true;

            var association = new WebsiteKeyword { WebsiteId = websiteId, KeywordId = keywordId };
            _context.WebsiteKeywords.Add(association);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DisassociateKeywordFromWebsiteAsync(int websiteId, int keywordId)
        {
            var association = await _context.WebsiteKeywords
                .FirstOrDefaultAsync(wk => wk.WebsiteId == websiteId && wk.KeywordId == keywordId);

            if (association == null) return false;

            _context.WebsiteKeywords.Remove(association);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<KeywordDto>> GetKeywordsForWebsiteAsync(int websiteId)
        {
            var website = await _context.Websites
                                .Include(w => w.WebsiteKeywords)
                                .ThenInclude(wk => wk.Keyword)
                                .FirstOrDefaultAsync(w => w.Id == websiteId);

            if (website == null) return null;

            return website.WebsiteKeywords.Select(wk => new KeywordDto
            {
                Id = wk.Keyword.Id,
                Text = wk.Keyword.Text
            }).ToList();
        }
    }
}
