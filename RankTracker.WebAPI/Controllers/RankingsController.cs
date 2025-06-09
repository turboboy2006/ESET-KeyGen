using Microsoft.AspNetCore.Mvc;
using RankTracker.WebAPI.DTOs;
using RankTracker.WebAPI.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RankTracker.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RankingsController : ControllerBase
    {
        private readonly IRankingService _rankingService;

        public RankingsController(IRankingService rankingService)
        {
            _rankingService = rankingService;
        }

        [HttpPost("check")]
        public async Task<ActionResult<RankingLogDto>> ManualCheck(ManualRankCheckRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _rankingService.PerformManualRankCheckAsync(request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception) // Consider logging the full exception
            {
                return StatusCode(500, "An error occurred while checking the rank.");
            }
        }

        [HttpGet("history")]
        public async Task<ActionResult<IEnumerable<RankingLogDto>>> GetHistory([FromQuery] RankingHistoryQueryDto query)
        {
            // Basic validation, could be enhanced with FluentValidation
            if (query.DateFrom.HasValue && query.DateTo.HasValue && query.DateFrom.Value > query.DateTo.Value)
            {
                return BadRequest("DateFrom cannot be after DateTo.");
            }
            var history = await _rankingService.GetRankingHistoryAsync(query);
            return Ok(history);
        }
    }
}
