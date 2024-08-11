using Microsoft.EntityFrameworkCore;
using RealEstateCore.Enums;
using RealEstateCore.Interfaces.V1;
using RealEstateCore.Models;
using RealEstateInfrastructure.Data;

namespace RealEstateInfrastructure.Repositories
{
    public class RealEstateRepository : IRealEstateRepository
    {
        private readonly RealEstateDbContext _context;

        public RealEstateRepository(RealEstateDbContext context)
        {
            _context = context;
        }

        public async Task<int> AddRealEstateAsync(RealEstate realEstate)
        {
            _context.RealEstates.Add(realEstate);
            await _context.SaveChangesAsync();
            return realEstate.Id;
        }

        public async Task ArchiveRealEstateAsync(int id)
        {
            var realEstate = await _context.RealEstates.FindAsync(id);
            if (realEstate != null)
            {
                realEstate.Status = RealEstateStatus.Archived;
                realEstate.ChangeLogs.Add(new ChangeLog
                {
                    ChangeDate = DateTime.UtcNow,
                    Description = "Property archived"
                });
                await _context.SaveChangesAsync();
            }
        }

        public async Task<RealEstate> GetRealEstateByIdAsync(int id)
        {
            return await _context.RealEstates
                .Include(r => r.Photos)
                .Include(r => r.ChangeLogs)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<RealEstate>> GetAllRealEstatesAsync()
        {
            return await _context.RealEstates
                .Include(r => r.Photos)
                .Include(r => r.ChangeLogs)
                //.Where(r => r.Status != RealEstateStatus.Archived)
                .OrderByDescending(r => r.UpdatedAt)
                .ToListAsync();
        }

        public async Task UpdateRealEstateAsync(RealEstate realEstate)
        {
            _context.RealEstates.Update(realEstate);
            await _context.SaveChangesAsync();
        }
    }
}
