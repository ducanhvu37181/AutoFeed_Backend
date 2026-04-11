using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Services.Interfaces;
using AutoFeed_Backend_Services.Models.Requests.FeedingRuleRequest;
using AutoFeed_Backend_Services.Models.Responses;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Services.Services
{
    public class FeedingRuleService : IFeedingRuleService
    {
        private readonly AutoFeedDBContext _context;
        public FeedingRuleService(AutoFeedDBContext context) => _context = context;

        // 1. Lấy danh sách tất cả Rules (Rút gọn)
        public async Task<IEnumerable<object>> GetAllRulesAsync()
        {
            return await _context.FeedingRules
                .Select(r => new {
                    r.RuleId,
                    r.Description,
                    r.Times
                })
                .ToListAsync();
        }

        // 2. Lấy chi tiết Rule kèm danh sách giờ ăn (Trả về DateOnly)
        public async Task<FeedingRuleFullResponse?> GetRuleByIdAsync(int id)
        {
            var rule = await _context.FeedingRules
                .Include(r => r.FeedingRuleDetails)
                .ThenInclude(d => d.Food)
                .FirstOrDefaultAsync(r => r.RuleId == id);

            if (rule == null) return null;

            return new FeedingRuleFullResponse
            {
                RuleId = rule.RuleId,
                Description = rule.Description,
                Times = rule.Times,
                Details = rule.FeedingRuleDetails.Select(d => new RuleDetailResponse
                {
                    FeedRuleDetailID = d.FeedRuleDetailId,
                    FoodID = d.FoodId,
                    FoodName = d.Food?.Name ?? "Unknown",
                    FeedHour = d.FeedHour,
                    FeedMinute = d.FeedMinute,
                    Status = d.Status ?? false,
                    // Trả về kiểu DateOnly để chỉ hiện Ngày/Tháng/Năm
                    StartDate = d.StartDate,
                    EndDate = d.EndDate
                }).ToList()
            };
        }

        // 3. Tạo Rule mới
        public async Task<bool> CreateRuleAsync(FeedingRuleCreateDto dto)
        {
            var rule = new FeedingRule
            {
                ChickenLid = dto.ChickenLid,
                FlockId = dto.FlockId,
                Times = dto.Times,
                Description = dto.Description,
                Note = dto.Note
            };
            _context.FeedingRules.Add(rule);
            return await _context.SaveChangesAsync() > 0;
        }

        // 4. Vô hiệu hóa chi tiết giờ ăn (Status = false)
        public async Task<bool> DisableDetailAsync(int detailId)
        {
            var detail = await _context.FeedingRuleDetails
                .FirstOrDefaultAsync(x => x.FeedRuleDetailId == detailId);

            if (detail == null) return false;

            detail.Status = false;
            _context.Entry(detail).State = EntityState.Modified;
            return await _context.SaveChangesAsync() > 0;
        }

        // 5. Cập nhật chi tiết giờ ăn (Dùng DateOnly trực tiếp)
        public async Task<bool> UpdateDetailAsync(int detailId, RuleDetailUpdateDto dto)
        {
            var detail = await _context.FeedingRuleDetails
                .FirstOrDefaultAsync(x => x.FeedRuleDetailId == detailId);

            if (detail == null) return false;

            detail.FoodId = dto.FoodID;
            // Gán trực tiếp vì DTO bây giờ đã là DateOnly
            detail.StartDate = dto.StartDate;
            detail.EndDate = dto.EndDate;

            detail.FeedHour = dto.FeedHour;
            detail.FeedMinute = dto.FeedMinute;
            detail.Description = dto.Description;

            _context.Entry(detail).State = EntityState.Modified;
            return await _context.SaveChangesAsync() > 0;
        }

        // 6. Xóa Rule
        public async Task<bool> DeleteRuleAsync(int id)
        {
            var rule = await _context.FeedingRules.FindAsync(id);
            if (rule == null) return false;
            _context.FeedingRules.Remove(rule);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}