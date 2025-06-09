using Microsoft.EntityFrameworkCore;
using RankTracker.Core.Data;
using RankTracker.Core.Models;
using RankTracker.WebAPI.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RankTracker.WebAPI.Services
{
    public class WebsiteService : IWebsiteService
    {
        private readonly RankTrackerDbContext _context;

        public WebsiteService(RankTrackerDbContext context)
        {
            _context = context;
        }

        public async Task<WebsiteDto> CreateWebsiteAsync(CreateWebsiteDto createWebsiteDto)
        {
            var website = new Website // In a real app, consider AutoMapper
            {
                Name = createWebsiteDto.Name,
                Url = createWebsiteDto.Url
            };

            _context.Websites.Add(website);
            await _context.SaveChangesAsync();

            return new WebsiteDto
            {
                Id = website.Id,
                Name = website.Name,
                Url = website.Url
            };
        }

        public async Task<IEnumerable<WebsiteDto>> GetAllWebsitesAsync()
        {
            return await _context.Websites
                .Select(w => new WebsiteDto // AutoMapper is good here
                {
                    Id = w.Id,
                    Name = w.Name,
                    Url = w.Url
                })
                .ToListAsync();
        }

        public async Task<WebsiteDto> GetWebsiteByIdAsync(int id)
        {
            var website = await _context.Websites.FindAsync(id);
            if (website == null)
            {
                return null;
            }

            return new WebsiteDto // AutoMapper helps
            {
                Id = website.Id,
                Name = website.Name,
                Url = website.Url
            };
        }

        public async Task<bool> UpdateWebsiteAsync(int id, UpdateWebsiteDto updateWebsiteDto)
        {
            var website = await _context.Websites.FindAsync(id);
            if (website == null)
            {
                return false;
            }

            website.Name = updateWebsiteDto.Name;
            website.Url = updateWebsiteDto.Url;

            _context.Entry(website).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteWebsiteAsync(int id)
        {
            var website = await _context.Websites.FindAsync(id);
            if (website == null)
            {
                return false;
            }

            _context.Websites.Remove(website);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
