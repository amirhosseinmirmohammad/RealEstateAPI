using Microsoft.Extensions.Logging;
using RealEstateCore.Enums;
using RealEstateCore.Interfaces.V1;
using RealEstateCore.Models;

namespace RealEstateApplication.Services.V1
{
    public class RealEstateCrudService
    {
        public RealEstateCrudService(IRealEstateRepository repository,
                                     ILogger<RealEstateCrudService> logger)
        {
            _repository = repository;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<int> AddRealEstateAsync(RealEstate realEstate, string userId)
        {
            realEstate.UserId = userId;
            realEstate.CreatedAt = DateTime.UtcNow;
            realEstate.UpdatedAt = DateTime.UtcNow;

            return await _repository.AddRealEstateAsync(realEstate);
        }

        public async Task ArchiveRealEstateAsync(int id)
        {
            var realEstate = await _repository.GetRealEstateByIdAsync(id);
            if (realEstate != null)
            {
                realEstate.Status = RealEstateStatus.Archived;
                realEstate.UpdatedAt = DateTime.UtcNow;

                realEstate.ChangeLogs.Add(new ChangeLog
                {
                    ChangeDate = DateTime.UtcNow,
                    Description = "Property archived"
                });

                await _repository.UpdateRealEstateAsync(realEstate);
                _logger.LogInformation("Archived RealEstate with ID {RealEstateId}", id);
            }
            else
            {
                _logger.LogWarning("RealEstate with ID {RealEstateId} not found", id);
            }
        }

        public async Task UpdateRealEstateTimeAsync(int id)
        {
            var realEstate = await _repository.GetRealEstateByIdAsync(id);
            if (realEstate != null)
            {
                realEstate.UpdatedAt = DateTime.UtcNow;

                realEstate.ChangeLogs.Add(new ChangeLog
                {
                    ChangeDate = DateTime.UtcNow,
                    Description = "Updated property time"
                });

                await _repository.UpdateRealEstateAsync(realEstate);
                _logger.LogInformation("Updated time for RealEstate with ID {RealEstateId}", id);
            }
            else
            {
                _logger.LogWarning("RealEstate with ID {RealEstateId} not found", id);
            }
        }

        public async Task<IEnumerable<RealEstate>> GetAllRealEstatesAsync()
        {
            var allRealEstates = await _repository.GetAllRealEstatesAsync();
            return allRealEstates
                //.Where(r => r.Status != RealEstateStatus.Archived)
                .OrderByDescending(r => r.UpdatedAt);
        }

        private readonly IRealEstateRepository _repository;
        private readonly ILogger<RealEstateCrudService> _logger;
    }
}
