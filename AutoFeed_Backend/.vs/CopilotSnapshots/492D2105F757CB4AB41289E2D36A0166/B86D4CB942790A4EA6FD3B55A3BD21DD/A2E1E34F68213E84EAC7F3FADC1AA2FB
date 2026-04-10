using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Repositories.UnitOfWork;
using AutoFeed_Backend_Services.Interfaces;
using AutoFeed_Backend.Models.Requests.Flock;
using AutoFeed_Backend.Models.Responses;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace AutoFeed_Backend_Services.Services
{
    public class FlockService : IFlockService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FlockService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // 1. Get all flocks
        public async Task<IEnumerable<FlockResponse>> GetAllFlocksAsync()
        {
            var flocks = await _unitOfWork.Flocks.GetAllAsync();
            if (flocks == null) return Enumerable.Empty<FlockResponse>();

            return flocks.Select(f => new FlockResponse
            {
                FlockId = f.FlockId,
                Name = f.Name,
                Quantity = f.Quantity,
                Weight = f.Weight,
                DoB = f.DoB,
                HealthStatus = f.HealthStatus,
                Note = f.Note
            });
        }

        // 2. Get flock by ID
        public async Task<FlockResponse?> GetFlockByIdAsync(int id)
        {
            var f = await _unitOfWork.Flocks.GetByIdAsync(id);
            if (f == null) return null;

            return new FlockResponse
            {
                FlockId = f.FlockId,
                Name = f.Name,
                Quantity = f.Quantity,
                Weight = f.Weight,
                DoB = f.DoB,
                HealthStatus = f.HealthStatus,
                Note = f.Note
            };
        }

        // 3. Create new flock
        public async Task<bool> CreateFlockAsync(FlockCreateRequest req)
        {
            try
            {
                var flock = new FlockChicken
                {
                    Name = req.Name,
                    Quantity = req.Quantity,
                    Weight = req.Weight,
                    DoB = req.DoB,
                    TransferDate = req.TransferDate,
                    HealthStatus = req.HealthStatus ?? "Healthy",
                    Note = req.Note
                };
                await _unitOfWork.Flocks.CreateAsync(flock);
                return await _unitOfWork.SaveChangesWithTransactionAsync();
            }
            catch (Exception) { return false; }
        }

        // 4. Update flock information
        public async Task<bool> UpdateFlockAsync(FlockUpdateRequest req)
        {
            var f = await _unitOfWork.Flocks.GetByIdAsync(req.FlockID);
            if (f == null) return false;

            f.Name = req.Name;
            f.Quantity = req.Quantity;
            f.Weight = req.Weight;
            f.HealthStatus = req.HealthStatus;
            f.Note = req.Note;

            await _unitOfWork.Flocks.UpdateAsync(f);
            return await _unitOfWork.SaveChangesWithTransactionAsync();
        }

        // 5. Delete flock
        public async Task<bool> DeleteFlockAsync(int id)
        {
            var f = await _unitOfWork.Flocks.GetByIdAsync(id);
            if (f == null) return false;

            await _unitOfWork.Flocks.RemoveAsync(f);
            return await _unitOfWork.SaveChangesWithTransactionAsync();
        }

        // 6. UPGRADE LOGIC: Move a chicken from Flock to Large Chicken Barn
        public async Task<bool> UpgradeToLargeChickenAsync(FlockUpgradeRequest req)
        {
            // Check if flock exists and has available chickens
            var flock = await _unitOfWork.Flocks.GetByIdAsync(req.FlockID);
            if (flock == null || flock.Quantity <= 0) return false;

            // Check if target barn exists
            var barn = await _unitOfWork.Barns.GetByIdAsync(req.BarnID);
            if (barn == null) return false;

            try
            {
                // 1. Create a record in LargeChicken table
                var lc = new LargeChicken
                {
                    FlockId = req.FlockID,
                    Name = req.ChickenName,
                    Weight = req.Weight,
                    HealthStatus = "Healthy",
                    Note = req.Note
                };
                await _unitOfWork.LargeChickens.CreateAsync(lc);
                await _unitOfWork.SaveChangesWithTransactionAsync(); // Save to get generated ChickenLid

                // 2. Assign the new large chicken to a specific barn
                var cb = new ChickenBarn
                {
                    BarnId = req.BarnID,
                    ChickenLid = lc.ChickenLid,
                    FlockId = req.FlockID,
                    StartDate = DateTime.Now,
                    Status = "Active",
                    Note = $"Upgraded from flock: {flock.Name}"
                };
                await _unitOfWork.ChickenBarns.CreateAsync(cb);

                // 3. Update the source flock: Decrease quantity by 1
                flock.Quantity -= 1;

                // If no chickens left, remove the flock record, otherwise update it
                if (flock.Quantity <= 0)
                    await _unitOfWork.Flocks.RemoveAsync(flock);
                else
                    await _unitOfWork.Flocks.UpdateAsync(flock);

                // Commit all changes
                return await _unitOfWork.SaveChangesWithTransactionAsync();
            }
            catch (Exception ex)
            {
                // Log the error for debugging
                Console.WriteLine("Error during chicken upgrade: " + ex.Message);
                return false;
            }
        }
    }
}