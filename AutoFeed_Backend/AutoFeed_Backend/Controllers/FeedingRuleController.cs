using Microsoft.AspNetCore.Mvc;
using AutoFeed_Backend_Services.Interfaces;
using AutoFeed_Backend_Services.Models.Requests.FeedingRuleRequest;
using System.Threading.Tasks;

namespace AutoFeed_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedingRuleController : ControllerBase
    {
        private readonly IFeedingRuleService _feedingRuleService;
        public FeedingRuleController(IFeedingRuleService feedingRuleService) => _feedingRuleService = feedingRuleService;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _feedingRuleService.GetAllRulesAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _feedingRuleService.GetRuleByIdAsync(id);
            return result == null ? NotFound("Rule not found") : Ok(result);
        }

        [HttpPost("detail")]
        public async Task<IActionResult> AddDetail([FromBody] RuleDetailCreateDto dto)
        {
            var (success, message) = await _feedingRuleService.AddDetailAsync(dto);
            return success ? Ok(new { success = true, message }) : BadRequest(new { success = false, message });
        }

        [HttpPost]
public async Task<IActionResult> Create(FeedingRuleCreateDto dto)
{
    try
    {
        var success = await _feedingRuleService.CreateRuleAsync(dto);
        return success ? Ok("Created") : BadRequest("Failed");
    }
    catch (Microsoft.EntityFrameworkCore.DbUpdateException ex) when (ex.InnerException?.Message.Contains("UIX_FRule_Flock") == true)
    {
        return BadRequest("Duplicate rule for this chickenLid and flockId.");
    }
    catch
    {
        return StatusCode(500, "Internal server error");
    }
}

        [HttpPut("detail/{detailId}/disable")]
        public async Task<IActionResult> DisableDetail(int detailId)
        {
            var success = await _feedingRuleService.DisableDetailAsync(detailId);
            return success ? Ok("Disabled") : NotFound();
        }

        [HttpPut("detail/{detailId}")]
        public async Task<IActionResult> UpdateDetail(int detailId, RuleDetailUpdateDto dto)
        {
            var success = await _feedingRuleService.UpdateDetailAsync(detailId, dto);
            return success ? Ok("Updated") : BadRequest();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, FeedingRuleUpdateDto dto)
        {
            var (success, message) = await _feedingRuleService.UpdateRuleAsync(id, dto);
            if (success)
                return Ok(new { success = true, message });
            if (message == "Rule not found")
                return NotFound(new { success = false, message });
            return BadRequest(new { success = false, message });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _feedingRuleService.DeleteRuleAsync(id);
            return success ? Ok("Deleted") : NotFound();
        }
    }
}