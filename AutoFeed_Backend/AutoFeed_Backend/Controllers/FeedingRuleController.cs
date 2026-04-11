using Microsoft.AspNetCore.Mvc;
using AutoFeed_Backend_Services.Interfaces;
using AutoFeed_Backend_Services.Models.Requests.FeedingRuleRequest;
using AutoFeed_Backend_Services.Models.Responses;
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

        [HttpPost]
        public async Task<IActionResult> Create(FeedingRuleCreateDto dto)
        {
            var success = await _feedingRuleService.CreateRuleAsync(dto);
            return success ? Ok("Created") : BadRequest("Failed");
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _feedingRuleService.DeleteRuleAsync(id);
            return success ? Ok("Deleted") : NotFound();
        }
    }
}