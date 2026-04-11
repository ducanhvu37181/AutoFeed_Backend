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

        // 1. Thêm mới chi tiết giờ ăn (Hàm Kiên vừa yêu cầu)
        public async Task<bool> AddDetailAsync(RuleDetailCreateDto dto)
        {
            var detail = new FeedingRuleDetail
            {
                RuleId = dto.RuleID,
                FoodId = dto.FoodID,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                FeedHour = dto.FeedHour,
                FeedMinute = dto.FeedMinute,
                Description = dto.Description,
                Status = true
            };

            _context.FeedingRuleDetails.Add(detail);
            return await _context.SaveChangesAsync() > 0;
        }

        // 2. Lấy danh sách tất cả Rules
        public async Task<IEnumerable<object>> GetAllRulesAsync()
        {
            return await _context.FeedingRules
                .Select(r => new { r.RuleId, r.Description, r.Times })
                .ToListAsync();
        }

        // 3. Lấy chi tiết Rule và các giờ ăn
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
                    StartDate = d.StartDate,
                    EndDate = d.EndDate
                }).ToList()
            };
        }

        // 4. Tạo Rule gốc
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

        // 5. Vô hiệu hóa (Status = false)
        public async Task<bool> DisableDetailAsync(int detailId)
        {
            var detail = await _context.FeedingRuleDetails.FindAsync(detailId);
            if (detail == null) return false;
            detail.Status = false;
            return await _context.SaveChangesAsync() > 0;
        }

        // 6. Cập nhật chi tiết
        public async Task<bool> UpdateDetailAsync(int detailId, RuleDetailUpdateDto dto)
        {
            var detail = await _context.FeedingRuleDetails.FindAsync(detailId);
            if (detail == null) return false;

            detail.FoodId = dto.FoodID;
            detail.StartDate = dto.StartDate;
            detail.EndDate = dto.EndDate;
            detail.FeedHour = dto.FeedHour;
            detail.FeedMinute = dto.FeedMinute;
            detail.Description = dto.Description;

            return await _context.SaveChangesAsync() > 0;
        }

        // 7. Xóa Rule
        public async Task<bool> DeleteRuleAsync(int id)
        {
            var rule = await _context.FeedingRules.FindAsync(id);
            if (rule == null) return false;
            _context.FeedingRules.Remove(rule);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}