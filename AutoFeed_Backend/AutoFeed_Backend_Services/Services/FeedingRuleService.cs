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

        // 1. Lấy danh sách tất cả Rules (Dùng cho trang chủ)
        public async Task<IEnumerable<object>> GetAllRulesAsync()
        {
            return await _context.FeedingRules
                .Select(r => new { r.RuleId, r.Description, r.Times })
                .ToListAsync();
        }

        // 2. Lấy chi tiết Rule và các bữa ăn (Dùng để lấy ID bữa ăn đi test Disable/Update)
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

        // 3. Tạo mới Rule gốc (Bảng cha)
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

        // 4. Cập nhật Rule gốc (Hàm Kiên vừa yêu cầu thêm)
        public async Task<bool> UpdateRuleAsync(int id, FeedingRuleUpdateDto dto)
        {
            var rule = await _context.FeedingRules.FirstOrDefaultAsync(r => r.RuleId == id);
            if (rule == null) return false;

            rule.ChickenLid = dto.ChickenLid;
            rule.FlockId = dto.FlockId;
            rule.Times = dto.Times;
            rule.Description = dto.Description;
            rule.Note = dto.Note;

            _context.FeedingRules.Update(rule);
            return await _context.SaveChangesAsync() > 0;
        }

        // 5. Thêm một bữa ăn mới vào Rule (Bảng con)
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

        // 6. Cập nhật thông tin một bữa ăn (Sửa giờ, thức ăn...)
        public async Task<bool> UpdateDetailAsync(int detailId, RuleDetailUpdateDto dto)
        {
            var detail = await _context.FeedingRuleDetails
                .FirstOrDefaultAsync(x => x.FeedRuleDetailId == detailId);

            if (detail == null) return false;

            detail.FoodId = dto.FoodID;
            detail.StartDate = dto.StartDate;
            detail.EndDate = dto.EndDate;
            detail.FeedHour = dto.FeedHour;
            detail.FeedMinute = dto.FeedMinute;
            detail.Description = dto.Description;

            _context.FeedingRuleDetails.Update(detail);
            return await _context.SaveChangesAsync() > 0;
        }

        // 7. Vô hiệu hóa bữa ăn (Hàm Kiên đang test lỗi - Đã sửa logic tìm ID)
        public async Task<bool> DisableDetailAsync(int detailId)
        {
            var detail = await _context.FeedingRuleDetails
                .FirstOrDefaultAsync(x => x.FeedRuleDetailId == detailId);

            if (detail == null) return false;

            detail.Status = false;
            _context.FeedingRuleDetails.Update(detail);
            return await _context.SaveChangesAsync() > 0;
        }

        // 8. Xóa Rule (Xóa cả cha lẫn con)
        public async Task<bool> DeleteRuleAsync(int id)
        {
            var rule = await _context.FeedingRules.FindAsync(id);
            if (rule == null) return false;
            _context.FeedingRules.Remove(rule);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}