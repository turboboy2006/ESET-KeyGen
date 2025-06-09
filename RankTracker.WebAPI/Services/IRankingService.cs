using RankTracker.WebAPI.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RankTracker.WebAPI.Services
{
    public interface IRankingService
    {
        Task<RankingLogDto> PerformManualRankCheckAsync(ManualRankCheckRequestDto request);
        Task<IEnumerable<RankingLogDto>> GetRankingHistoryAsync(RankingHistoryQueryDto query);
    }
}
