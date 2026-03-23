using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoFeed_Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BarnController : ControllerBase
{
    private readonly IBarnService _barnService;

    public BarnController(IBarnService barnService)
    {
        _barnService = barnService;
    }

    // API lấy danh sách toàn bộ chuồng gà
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Barn>>> GetAllBarns()
    {
        var barns = await _barnService.GetAllAsync();
        return Ok(barns);
    }

    // API lấy chi tiết 1 chuồng gà theo ID
    [HttpGet("{id}")]
    public async Task<ActionResult<Barn>> GetBarnById(int id)
    {
        var barn = await _barnService.GetByIdAsync(id);
        if (barn == null) return NotFound("Không tìm thấy chuồng gà này.");
        return Ok(barn);
    }

    // API tạo mới một chuồng gà
    [HttpPost]
    public async Task<ActionResult<bool>> CreateBarn(Barn barn)
    {
        var result = await _barnService.CreateBarnAsync(barn);
        if (!result) return BadRequest("Tạo chuồng gà thất bại.");
        return Ok(result);
    }

    // API cập nhật thông tin chuồng gà
    [HttpPut]
    public async Task<ActionResult<bool>> UpdateBarn(Barn barn)
    {
        var result = await _barnService.UpdateBarnAsync(barn);
        if (!result) return BadRequest("Cập nhật thất bại.");
        return Ok(result);
    }

    // API xóa chuồng gà
    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteBarn(int id)
    {
        var result = await _barnService.DeleteBarnAsync(id);
        if (!result) return BadRequest("Xóa thất bại.");
        return Ok(result);
    }
}