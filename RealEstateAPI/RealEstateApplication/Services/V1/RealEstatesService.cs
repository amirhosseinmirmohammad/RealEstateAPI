using Microsoft.Extensions.Logging;
using RealEstateCore.Enums;
using RealEstateCore.Interfaces.V1;
using RealEstateCore.Models;
using RealEstateService.ViewModels.RealEstateAPI.ViewModels;

namespace RealEstateApplication.Services.V1
{
    public class RealEstatesService
    {
        public RealEstatesService(IRealEstateRepository repository,
                                     ILogger<RealEstatesService> logger)
        {
            _repository = repository;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ResponseModel<int>> AddRealEstateAsync(RealEstateViewModel viewModel)
        {
            try
            {
                var realEstate = new RealEstate
                {
                    Title = viewModel.Title,
                    Status = viewModel.Status,
                    Price = viewModel.Price,
                    Floor = viewModel.Floor,
                    UserId = "UserId", 
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var id = await _repository.AddRealEstateAsync(realEstate);
                return new ResponseModel<int>
                {
                    StatusCode = 200,
                    Data = id,
                    Message = "Real estate added successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding real estate");
                return new ResponseModel<int>
                {
                    StatusCode = 500,
                    Data = 0,
                    Message = "Error adding real estate",
                    Exception = ex.Message
                };
            }
        }

        public async Task<ResponseModel<int>> ArchiveRealEstateAsync(int id)
        {
            try
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

                    return new ResponseModel<int>
                    {
                        StatusCode = 200,
                        Data = id,
                        Message = "Real estate archived successfully"
                    };
                }

                return new ResponseModel<int>
                {
                    StatusCode = 404,
                    Data = 0,
                    Message = "Real estate not found"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving real estate");
                return new ResponseModel<int>
                {
                    StatusCode = 500,
                    Data = 0,
                    Message = "Error archiving real estate",
                    Exception = ex.Message
                };
            }
        }

        public async Task<ResponseModel<int>> UpdateRealEstateTimeAsync(int id)
        {
            try
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

                    return new ResponseModel<int>
                    {
                        StatusCode = 200,
                        Data = id,
                        Message = "Real estate time updated successfully"
                    };
                }

                return new ResponseModel<int>
                {
                    StatusCode = 404,
                    Data = 0,
                    Message = "Real estate not found"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating real estate time");
                return new ResponseModel<int>
                {
                    StatusCode = 500,
                    Data = 0,
                    Message = "Error updating real estate time",
                    Exception = ex.Message
                };
            }
        }

        public async Task<ResponseModel<IEnumerable<RealEstate>>> GetAllRealEstatesAsync()
        {
            try
            {
                var allRealEstates = await _repository.GetAllRealEstatesAsync();
                return new ResponseModel<IEnumerable<RealEstate>>
                {
                    StatusCode = 200,
                    Data = allRealEstates.OrderByDescending(r => r.UpdatedAt),
                    Message = "Real estate list retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving real estate list");
                return new ResponseModel<IEnumerable<RealEstate>>
                {
                    StatusCode = 500,
                    Data = null,
                    Message = "Error retrieving real estate list",
                    Exception = ex.Message
                };
            }
        }

        private readonly IRealEstateRepository _repository;
        private readonly ILogger<RealEstatesService> _logger;
    }
}
