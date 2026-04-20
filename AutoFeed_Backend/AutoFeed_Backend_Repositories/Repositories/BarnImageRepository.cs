using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Repositories.BasicRepo;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Repositories.Repositories;

public class BarnImageRepository : GenericRepository<BarnImage>
{
    public BarnImageRepository() : base() { }

    public BarnImageRepository(AutoFeedDBContext context) : base(context) { }

    // Get all barn images with related barn information
    public async Task<List<BarnImage>> GetAllWithDetailsAsync()
    {
        return await _context.BarnImages
            .Include(bi => bi.Barn)
            .OrderByDescending(bi => bi.ImageBarnId)
            .ToListAsync();
    }

    // Get barn images by BarnId
    public async Task<List<BarnImage>> GetByBarnIdAsync(int barnId)
    {
        return await _context.BarnImages
            .Include(bi => bi.Barn)
            .Where(bi => bi.BarnId == barnId)
            .OrderByDescending(bi => bi.ImageBarnId)
            .ToListAsync();
    }

    // Get single barn image by ImageBarnId
    public async Task<BarnImage> GetByImageIdAsync(int imageBarnId)
    {
        return await _context.BarnImages
            .Include(bi => bi.Barn)
            .FirstOrDefaultAsync(bi => bi.ImageBarnId == imageBarnId);
    }
}
