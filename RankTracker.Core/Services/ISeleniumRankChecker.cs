using System.Threading.Tasks;

namespace RankTracker.Core.Services
{
    public interface ISeleniumRankChecker
    {
        Task<(int? Rank, string Notes)> GetRankAsync(string targetUrl, string keyword, int maxPagesToCheck = 10);
    }
}
