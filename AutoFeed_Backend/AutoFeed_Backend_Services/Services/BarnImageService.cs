using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Repositories.UnitOfWork;
using AutoFeed_Backend_Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Services.Services;

public class BarnImageService : IBarnImageService
{
    private readonly IUnitOfWork _unitOfWork;

    public BarnImageService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<object>> GetAllBarnImagesAsync()
    {
        var images = await _unitOfWork.BarnImages.GetAllWithDetailsAsync();

        return images.Select(bi => new
        {
            ImageBarnId = bi.ImageBarnId,
            BarnId = bi.BarnId,
            BarnType = bi.Barn?.Type,
            Url = bi.Url,
            Description = bi.Description,
            CaptureDate = bi.CaptureDate
        });
    }

    public async Task<object> GetBarnImageByIdAsync(int imageBarnId)
    {
        var image = await _unitOfWork.BarnImages.GetByImageIdAsync(imageBarnId);

        if (image == null)
            return null;

        return new
        {
            ImageBarnId = image.ImageBarnId,
            BarnId = image.BarnId,
            BarnType = image.Barn?.Type,
            Url = image.Url,
            Description = image.Description,
            CaptureDate = image.CaptureDate
        };
    }

    public async Task<IEnumerable<object>> GetBarnImagesByBarnIdAsync(int barnId)
    {
        var images = await _unitOfWork.BarnImages.GetByBarnIdAsync(barnId);

        return images.Select(bi => new
        {
            ImageBarnId = bi.ImageBarnId,
            BarnId = bi.BarnId,
            BarnType = bi.Barn?.Type,
            Url = bi.Url,
            Description = bi.Description,
            CaptureDate = bi.CaptureDate
        });
    }

    public async Task<BarnImage> AddBarnImageAsync(int barnId, string description)
    {
        // Validate that barn exists
        var barn = await _unitOfWork.Barns.GetByIdAsync(barnId);
        if (barn == null)
            throw new Exception($"Barn with ID {barnId} not found");

        var barnImage = new BarnImage
        {
            BarnId = barnId,
            Description = description,
            Url = string.Empty, // Will be updated after Firebase upload
            CaptureDate = DateTime.Now
        };

        await _unitOfWork.BarnImages.CreateAsync(barnImage);
        await _unitOfWork.SaveChangesWithTransactionAsync();

        return barnImage;
    }

    // Update barn image URL after Firebase upload
    public async Task<bool> UpdateBarnImageUrlAsync(int imageBarnId, string url)
    {
        try
        {
            // Get the image from database  
            var image = await _unitOfWork.BarnImages.GetByImageIdAsync(imageBarnId);
            if (image == null)
                return false;

            // Update the URL on the tracked entity
            image.Url = url;

            // Save the changes
            await _unitOfWork.SaveChangesWithTransactionAsync();
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
}
