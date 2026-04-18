using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Services.Interfaces;
using AutoFeed_Backend_Services.Models.Responses;
using AutoFeed_Backend.Models.Responses;
using AutoFeed_Backend.Models.Requests.Food;
using Microsoft.AspNetCore.Mvc;

namespace AutoFeed_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FoodController : ControllerBase
    {
        private readonly IFoodService _foodService;

        public FoodController(IFoodService foodService)
        {
            _foodService = foodService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var foods = await _foodService.GetAllFoodsAsync();

            var response = foods.Select(f => new FoodResponse
            {
                FoodId = f.FoodId.ToString(),
                Name = f.Name,
                Type = f.Type,
                Note = f.Note
            }).ToList();

            return Ok(new ApiResponse<object>
            {
                Status = true,
                HttpCode = 200,
                Data = response,
                Description = "Foods retrieved successfully."
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FoodCreateRequest request)
        {
            var food = new Food { Name = request.Name, Type = request.Type, Note = request.Note };
            await _foodService.AddFoodItemAsync(food);

            var response = new FoodResponse
            {
                FoodId = food.FoodId.ToString(),
                Name = food.Name,
                Type = food.Type,
                Note = food.Note
            };

            return Ok(new ApiResponse<object>
            {
                Status = true,
                HttpCode = 200,
                Data = response,
                Description = "Add sucessfully!!"
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] FoodUpdateRequest request)
        {
            if (id != request.FoodId)
                return BadRequest(new ApiResponse<object>
                {
                    Status = false,
                    HttpCode = 400,
                    Data = null,
                    Description = "ID mismatch!"
                });

            var food = new Food
            {
                FoodId = request.FoodId,
                Name = request.Name,
                Type = request.Type,
                Note = request.Note
            };

            var result = await _foodService.UpdateFoodAsync(food);
            if (result)
                return Ok(new ApiResponse<object>
                {
                    Status = true,
                    HttpCode = 200,
                    Data = null,
                    Description = "Food item updated successfully!"
                });

            return NotFound(new ApiResponse<object>
            {
                Status = false,
                HttpCode = 404,
                Data = null,
                Description = "Food item not found!"
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _foodService.DeleteFoodAsync(id);
                if (result)
                    return Ok(new ApiResponse<object>
                    {
                        Status = true,
                        HttpCode = 200,
                        Data = null,
                        Description = "Food item deleted successfully!"
                    });

                return BadRequest(new ApiResponse<object>
                {
                    Status = false,
                    HttpCode = 400,
                    Data = null,
                    Description = "Delete failed"
                });
            }
            catch
            {
                return BadRequest(new ApiResponse<object>
                {
                    Status = false,
                    HttpCode = 400,
                    Data = null,
                    Description = "Delete failed"
                });
            }
        }
    }
}