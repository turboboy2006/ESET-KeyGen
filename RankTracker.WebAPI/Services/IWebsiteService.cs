using RankTracker.WebAPI.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RankTracker.WebAPI.Services
{
    public interface IWebsiteService
    {
        Task<WebsiteDto> CreateWebsiteAsync(CreateWebsiteDto createWebsiteDto);
        Task<WebsiteDto> GetWebsiteByIdAsync(int id);
        Task<IEnumerable<WebsiteDto>> GetAllWebsitesAsync();
        Task<bool> UpdateWebsiteAsync(int id, UpdateWebsiteDto updateWebsiteDto);
        Task<bool> DeleteWebsiteAsync(int id);
    }
}
