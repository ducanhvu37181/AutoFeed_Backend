using Microsoft.AspNetCore.Mvc;
using AutoFeed_Backend_Services.Interfaces;
using AutoFeed_Backend_Services.Models.Requests.FeedingRuleRequest;
using AutoFeed_Backend.Models.Responses;
using System.Threading.Tasks;

namespace AutoFeed_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedingRuleController : ControllerBase
    {
        private readonly IFeedingRuleService _feedingRuleService;
        private readonly IFlockService _flockService;
        private readonly ILargeChickenService _largeChickenService;

        public FeedingRuleController(IFeedingRuleService feedingRuleService, IFlockService flockService, ILargeChickenService largeChickenService)
        {
            _feedingRuleService = feedingRuleService;
            _flockService = flockService;
            _largeChickenService = largeChickenService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _feedingRuleService.GetAllRulesAsync());

        [HttpGet("barn/{barnId}")]
        public async Task<IActionResult> GetByBarn(int barnId)
        {
            var result = await _feedingRuleService.GetRulesByBarnIdAsync(barnId);
            return Ok(result);
        }

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
            // Check if only one of chickenLid or flockId is provided
            bool hasChicken = dto.ChickenLid != null;
            bool hasFlock = dto.FlockId != null;
            if (hasChicken == hasFlock) // both are null or both have values
            {
                return BadRequest(new ApiResponse<object>
                {
                    Status = false,
                    HttpCode = 400,
                    Data = null,
                    Description = "You must provide exactly one of chickenLid or flockId. Providing both or neither is not allowed."
                });
            }

            // Validate Times > 0
            if (dto.Times <= 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Status = false,
                    HttpCode = 400,
                    Data = null,
                    Description = "Times must be greater than 0"
                });
            }

            // Validate StartDate <= EndDate
            if (dto.StartDate > dto.EndDate)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Status = false,
                    HttpCode = 400,
                    Data = null,
                    Description = "Invalid date range: StartDate must be less than or equal to EndDate"
                });
            }

            // Validate ChickenLid existence if provided
            if (dto.ChickenLid.HasValue)
            {
                var chickenExists = await _largeChickenService.GetByIdAsync(dto.ChickenLid.Value);
                if (chickenExists == null)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Status = false,
                        HttpCode = 400,
                        Data = null,
                        Description = "ChickenLid not found"
                    });
                }
            }

            // Validate FlockId existence if provided
            if (dto.FlockId.HasValue)
            {
                var flockExists = await _flockService.GetFlockByIdAsync(dto.FlockId.Value);
                if (flockExists == null)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Status = false,
                        HttpCode = 400,
                        Data = null,
                        Description = "FlockId not found"
                    });
                }
            }

            try
            {
                var success = await _feedingRuleService.CreateRuleAsync(dto);
                if (success)
                {
                    return Ok(new ApiResponse<object>
                    {
                        Status = true,
                        HttpCode = 201,
                        Data = null,
                        Description = "Feeding rule created successfully!"
                    });
                }
                return BadRequest(new ApiResponse<object>
                {
                    Status = false,
                    HttpCode = 400,
                    Data = null,
                    Description = "Failed to create feeding rule"
                });
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex) when (ex.InnerException?.Message.Contains("UIX_FRule_Flock") == true)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Status = false,
                    HttpCode = 400,
                    Data = null,
                    Description = "Duplicate rule for this chickenLid and flockId."
                });
            }
            catch
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Status = false,
                    HttpCode = 500,
                    Data = null,
                    Description = "Internal server error"
                });
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