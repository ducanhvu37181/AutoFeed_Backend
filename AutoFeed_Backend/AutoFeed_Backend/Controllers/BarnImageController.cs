using AutoFeed_Backend_Services.Interfaces;
using AutoFeed_Backend.Helpers;
using AutoFeed_Backend.Models.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace AutoFeed_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BarnImageController : ControllerBase
{
    private readonly IBarnImageService _service;

    public BarnImageController(IBarnImageService service)
    {
        _service = service;
    }

    // POST api/BarnImage/{barnId}/upload
    // Upload barn image to Firebase and save to database
    [HttpPost("{barnId:int}/upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadBarnImage(
        int barnId,
        IFormFile file,
        [FromForm] string description,
        [FromServices] IConfiguration config)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new ApiResponse<object>
            {
                Status = false,
                HttpCode = 400,
                Data = null,
                Description = "Invalid file"
            });

        if (barnId <= 0)
            return BadRequest(new ApiResponse<object>
            {
                Status = false,
                HttpCode = 400,
                Data = null,
                Description = "Invalid barnId"
            });

        var bucket = config.GetValue<string>("Firebase:Bucket");
        var saPath = config.GetValue<string>("Firebase:ServiceAccountPath");
        if (string.IsNullOrWhiteSpace(bucket) || string.IsNullOrWhiteSpace(saPath))
            return StatusCode(500, new ApiResponse<object>
            {
                Status = false,
                HttpCode = 500,
                Data = null,
                Description = "Firebase not configured"
            });

        try
        {
            // Create barn image record first (not saved yet)
            var barnImage = await _service.AddBarnImageAsync(barnId, description ?? "");

            // Upload file to Firebase
            var url = await FirebaseStorageHelper.UploadFileAndGetSignedUrlAsync(
                file,
                bucket,
                saPath,
                TimeSpan.FromDays(365));

            // Update barn image URL (will save on first create + URL update)
            barnImage.Url = url;
            var updated = await _service.UpdateBarnImageUrlAsync(barnImage.ImageBarnId, url);
            if (!updated)
                return StatusCode(500, new ApiResponse<object>
                {
                    Status = false,
                    HttpCode = 500,
                    Data = null,
                    Description = "Failed to save image"
                });

            return Ok(new ApiResponse<object>
            {
                Status = true,
                HttpCode = 200,
                Data = new
                {
                    ImageBarnId = barnImage.ImageBarnId,
                    BarnId = barnImage.BarnId,
                    Url = url,
                    Description = barnImage.Description
                },
                Description = "Barn image uploaded successfully"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Status = false,
                HttpCode = 400,
                Data = null,
                Description = ex.Message
            });
        }
    }

    // GET api/BarnImage
    // Get all barn images
    [HttpGet]
    public async Task<IActionResult> GetAllBarnImages()
    {
        try
        {
            var data = await _service.GetAllBarnImagesAsync();

            return Ok(new ApiResponse<object>
            {
                Status = true,
                HttpCode = 200,
                Data = data,
                Description = "Success"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Status = false,
                HttpCode = 400,
                Data = null,
                Description = ex.Message
            });
        }
    }

    // GET api/BarnImage/{imageBarnId}
    // Get barn image by ImageBarnId
    [HttpGet("{imageBarnId:int}")]
    public async Task<IActionResult> GetBarnImageById(int imageBarnId)
    {
        if (imageBarnId <= 0)
            return BadRequest(new ApiResponse<object>
            {
                Status = false,
                HttpCode = 400,
                Data = null,
                Description = "Invalid imageBarnId"
            });

        try
        {
            var data = await _service.GetBarnImageByIdAsync(imageBarnId);

            if (data == null)
                return NotFound(new ApiResponse<object>
                {
                    Status = false,
                    HttpCode = 404,
                    Data = null,
                    Description = "Barn image not found"
                });

            return Ok(new ApiResponse<object>
            {
                Status = true,
                HttpCode = 200,
                Data = data,
                Description = "Success"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Status = false,
                HttpCode = 400,
                Data = null,
                Description = ex.Message
            });
        }
    }

    // GET api/BarnImage/barn/{barnId}
    // Get all barn images by BarnId
    [HttpGet("barn/{barnId:int}")]
    public async Task<IActionResult> GetBarnImagesByBarnId(int barnId)
    {
        if (barnId <= 0)
            return BadRequest(new ApiResponse<object>
            {
                Status = false,
                HttpCode = 400,
                Data = null,
                Description = "Invalid barnId"
            });

        try
        {
            var data = await _service.GetBarnImagesByBarnIdAsync(barnId);

            return Ok(new ApiResponse<object>
            {
                Status = true,
                HttpCode = 200,
                Data = data,
                Description = "Success"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Status = false,
                HttpCode = 400,
                Data = null,
                Description = ex.Message
            });
        }
    }
}
