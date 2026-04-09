using Microsoft.AspNetCore.Mvc;
using AutoFeed_Backend_Services.Interfaces;
using AutoFeed_Backend_DAO.Models;

namespace AutoFeed_Backend_API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FeedingRuleController : ControllerBase
{
    private readonly IFeedingRuleService _feedingRuleService;

    public FeedingRuleController(IFeedingRuleService feedingRuleService)
    {
        _feedingRuleService = feedingRuleService;
    }

    // 1. GET: api/FeedingRule (Lấy danh sách các quy tắc ăn uống)
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var rules = await _feedingRuleService.GetAllRulesAsync();
        return Ok(rules);
    }

    // 2. GET: api/FeedingRule/{id} (Chi tiết 1 quy tắc)
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var rule = await _feedingRuleService.GetRuleByIdAsync(id);
        if (rule == null) return NotFound("Không tìm thấy quy tắc yêu cầu.");
        return Ok(rule);
    }

    // 3. POST: api/FeedingRule (Tạo quy tắc mới - Chicks, Fighting, hoặc Sick)
    [HttpPost]
    public async Task<IActionResult> Create(FeedingRuleCreateDto dto)
    {
        var result = await _feedingRuleService.CreateRuleAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.RuleId }, result);
    }

    // 4. PUT: api/FeedingRule/{id} (Cập nhật quy tắc)
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, FeedingRule rule)
    {
        var isUpdated = await _feedingRuleService.UpdateRuleAsync(id, rule);
        if (!isUpdated) return BadRequest("Cập nhật thất bại. Vui lòng kiểm tra lại ID.");
        return Ok("Cập nhật quy tắc thành công.");
    }

    // 5. DELETE: api/FeedingRule/{id} (Xóa quy tắc)
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var isDeleted = await _feedingRuleService.DeleteRuleAsync(id);
        if (!isDeleted) return NotFound("Không tìm thấy quy tắc để xóa.");
        return Ok("Đã xóa quy tắc thành công.");
    }
}