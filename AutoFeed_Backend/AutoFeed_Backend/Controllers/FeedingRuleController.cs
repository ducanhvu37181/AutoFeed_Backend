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
        private readonly IFoodService _foodService;

        public FeedingRuleController(IFeedingRuleService feedingRuleService, IFlockService flockService, ILargeChickenService largeChickenService, IFoodService foodService)
        {
            _feedingRuleService = feedingRuleService;
            _flockService = flockService;
            _largeChickenService = largeChickenService;
            _foodService = foodService;
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
            // Validate RuleID > 0
            if (dto.RuleID <= 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Status = false,
                    HttpCode = 400,
                    Data = null,
                    Description = "RuleID must be greater than 0"
                });
            }

            // Validate RuleID exists
            var ruleExists = await _feedingRuleService.GetRuleByIdAsync(dto.RuleID);
            if (ruleExists == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Status = false,
                    HttpCode = 400,
                    Data = null,
                    Description = "RuleID not found"
                });
            }

            // Validate FoodID > 0
            if (dto.FoodID <= 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Status = false,
                    HttpCode = 400,
                    Data = null,
                    Description = "FoodID must be greater than 0"
                });
            }

            // Validate FoodID exists
            var foodExists = await _foodService.GetFoodByIdAsync(dto.FoodID);
            if (foodExists == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Status = false,
                    HttpCode = 400,
                    Data = null,
                    Description = "FoodID not found"
                });
            }

            // Validate Amount > 0
            if (dto.Amount <= 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Status = false,
                    HttpCode = 400,
                    Data = null,
                    Description = "Amount must be greater than 0"
                });
            }

            // Validate FeedHour range (0-23)
            if (dto.FeedHour < 0 || dto.FeedHour > 23)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Status = false,
                    HttpCode = 400,
                    Data = null,
                    Description = "FeedHour must be between 0 and 23"
                });
            }

            // Validate FeedMinute range (0-59)
            if (dto.FeedMinute < 0 || dto.FeedMinute > 59)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Status = false,
                    HttpCode = 400,
                    Data = null,
                    Description = "FeedMinute must be between 0 and 59"
                });
            }

            // Validate Description not empty
            if (string.IsNullOrWhiteSpace(dto.Description))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Status = false,
                    HttpCode = 400,
                    Data = null,
                    Description = "Description cannot be empty"
                });
            }

            try
            {
                var (success, message) = await _feedingRuleService.AddDetailAsync(dto);
                if (success)
                {
                    return Ok(new ApiResponse<object>
                    {
                        Status = true,
                        HttpCode = 201,
                        Data = null,
                        Description = message ?? "Feeding rule detail created successfully!"
                    });
                }
                return BadRequest(new ApiResponse<object>
                {
                    Status = false,
                    HttpCode = 400,
                    Data = null,
                    Description = message ?? "Failed to create feeding rule detail"
                });
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate") == true || ex.InnerException?.Message.Contains("unique") == true)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Status = false,
                    HttpCode = 400,
                    Data = null,
                    Description = "Duplicate feeding rule detail for this rule and time."
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