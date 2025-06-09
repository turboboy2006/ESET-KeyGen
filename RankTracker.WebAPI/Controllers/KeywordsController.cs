using Microsoft.AspNetCore.Mvc;
using RankTracker.WebAPI.DTOs;
using RankTracker.WebAPI.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RankTracker.WebAPI.Controllers
{
    [Route("api")]
    [ApiController]
    public class KeywordsController : ControllerBase
    {
        private readonly IKeywordService _keywordService;

        public KeywordsController(IKeywordService keywordService)
        {
            _keywordService = keywordService;
        }

        [HttpPost("keywords")]
        public async Task<ActionResult<KeywordDto>> PostKeyword(CreateKeywordDto createKeywordDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var keyword = await _keywordService.CreateKeywordAsync(createKeywordDto);
            // Assuming GetKeyword action exists for retrieving a single keyword by ID
            return CreatedAtAction(nameof(GetKeyword), new { id = keyword.Id }, keyword);
        }

        [HttpGet("keywords")]
        public async Task<ActionResult<IEnumerable<KeywordDto>>> GetKeywords()
        {
            var keywords = await _keywordService.GetAllKeywordsAsync();
            return Ok(keywords);
        }

        [HttpGet("keywords/{id}")]
        public async Task<ActionResult<KeywordDto>> GetKeyword(int id) // Name matches CreatedAtAction
        {
            var keyword = await _keywordService.GetKeywordByIdAsync(id);
            if (keyword == null) return NotFound();
            return Ok(keyword);
        }

        [HttpDelete("keywords/{id}")]
        public async Task<IActionResult> DeleteKeyword(int id)
        {
            var result = await _keywordService.DeleteKeywordAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpPost("websites/{websiteId}/keywords/{keywordId}")]
        public async Task<IActionResult> AssociateKeyword(int websiteId, int keywordId)
        {
            var result = await _keywordService.AssociateKeywordWithWebsiteAsync(websiteId, keywordId);
            if (!result) return NotFound("Website or Keyword not found, or association failed.");
            return Ok();
        }

        [HttpDelete("websites/{websiteId}/keywords/{keywordId}")]
        public async Task<IActionResult> DisassociateKeyword(int websiteId, int keywordId)
        {
            var result = await _keywordService.DisassociateKeywordFromWebsiteAsync(websiteId, keywordId);
            if (!result) return NotFound("Association not found.");
            return NoContent();
        }

        [HttpGet("websites/{websiteId}/keywords")]
        public async Task<ActionResult<IEnumerable<KeywordDto>>> GetKeywordsForWebsite(int websiteId)
        {
            var keywords = await _keywordService.GetKeywordsForWebsiteAsync(websiteId);
            if (keywords == null) return NotFound("Website not found.");
            return Ok(keywords);
        }
    }
}
