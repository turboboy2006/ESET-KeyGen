using Microsoft.AspNetCore.Mvc;
using RankTracker.WebAPI.DTOs;
using RankTracker.WebAPI.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RankTracker.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebsitesController : ControllerBase
    {
        private readonly IWebsiteService _websiteService;

        public WebsitesController(IWebsiteService websiteService)
        {
            _websiteService = websiteService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<WebsiteDto>>> GetWebsites()
        {
            var websites = await _websiteService.GetAllWebsitesAsync();
            return Ok(websites);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<WebsiteDto>> GetWebsite(int id)
        {
            var website = await _websiteService.GetWebsiteByIdAsync(id);
            if (website == null)
            {
                return NotFound();
            }
            return Ok(website);
        }

        [HttpPost]
        public async Task<ActionResult<WebsiteDto>> PostWebsite(CreateWebsiteDto createWebsiteDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var createdWebsite = await _websiteService.CreateWebsiteAsync(createWebsiteDto);
            return CreatedAtAction(nameof(GetWebsite), new { id = createdWebsite.Id }, createdWebsite);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutWebsite(int id, UpdateWebsiteDto updateWebsiteDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _websiteService.UpdateWebsiteAsync(id, updateWebsiteDto);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWebsite(int id)
        {
            var result = await _websiteService.DeleteWebsiteAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
