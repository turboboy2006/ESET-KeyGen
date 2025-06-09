using RankTracker.WebAPI.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RankTracker.WebAPI.Services
{
    public interface IKeywordService
    {
        Task<KeywordDto> CreateKeywordAsync(CreateKeywordDto createKeywordDto);
        Task<KeywordDto> GetKeywordByIdAsync(int id);
        Task<IEnumerable<KeywordDto>> GetAllKeywordsAsync();
        Task<bool> DeleteKeywordAsync(int id);
        Task<bool> AssociateKeywordWithWebsiteAsync(int websiteId, int keywordId);
        Task<bool> DisassociateKeywordFromWebsiteAsync(int websiteId, int keywordId);
        Task<IEnumerable<KeywordDto>> GetKeywordsForWebsiteAsync(int websiteId);
    }
}
