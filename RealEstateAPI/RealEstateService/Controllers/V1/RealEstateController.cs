using Microsoft.AspNetCore.Mvc;
using RealEstateApplication.Services.V1;
using RealEstateCore.Models;
using RealEstateService.ViewModels.RealEstateAPI.ViewModels;
using System.Security.Claims;

namespace RealEstateService.Controllers.V1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class RealEstateController : ControllerBase
    {
        public RealEstateController(RealEstateCrudService service,
                                    ILogger<RealEstateController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddRealEstate([FromBody] RealEstateViewModel realEstateViewModel)
        {
            _logger.LogInformation("AddRealEstate API called");
            try
            {
                string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "UserId";

                var realEstate = new RealEstate
                {
                    Title = realEstateViewModel.Title,
                    Status = realEstateViewModel.Status,
                    Price = realEstateViewModel.Price,
                    Floor = realEstateViewModel.Floor,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    UserId = userId
                };

                var id = await _service.AddRealEstateAsync(realEstate, userId);

                _logger.LogInformation("Real estate added successfully with ID {RealEstateId}", id);
                return Ok(new ResponseModel<int>
                {
                    StatusCode = 200,
                    Data = id,
                    Message = "Real estate added successfully",
                    Exception = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding real estate");
                return BadRequest(new ResponseModel<object>
                {
                    StatusCode = 400,
                    Data = null,
                    Message = "Error adding real estate",
                    Exception = ex.Message
                });
            }
        }

        [HttpPut("update-time/{id}")]
        public async Task<IActionResult> UpdateRealEstateTime(int id)
        {
            _logger.LogInformation("UpdateRealEstateTime API called for RealEstateId: {RealEstateId}", id);
            try
            {
                await _service.UpdateRealEstateTimeAsync(id);
                return Ok(new ResponseModel<int>
                {
                    StatusCode = 200,
                    Data = id,
                    Message = "Real estate time updated successfully",
                    Exception = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating real estate time with ID {RealEstateId}", id);
                return BadRequest(new ResponseModel<object>
                {
                    StatusCode = 400,
                    Data = null,
                    Message = "Error updating real estate time",
                    Exception = ex.Message
                });
            }
        }

        [HttpPut("archive/{id}")]
        public async Task<IActionResult> ArchiveRealEstate(int id)
        {
            _logger.LogInformation("ArchiveRealEstate API called for RealEstateId: {RealEstateId}", id);
            try
            {
                await _service.ArchiveRealEstateAsync(id);
                _logger.LogInformation("Real estate archived successfully with ID {RealEstateId}", id);
                return Ok(new ResponseModel<int>
                {
                    StatusCode = 200,
                    Data = id,
                    Message = "Real estate archived successfully",
                    Exception = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving real estate with ID {RealEstateId}", id);
                return BadRequest(new ResponseModel<object>
                {
                    StatusCode = 400,
                    Data = null,
                    Message = "Error archiving real estate",
                    Exception = ex.Message
                });
            }
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetAllRealEstates()
        {
            _logger.LogInformation("GetAllRealEstates API called");
            try
            {
                var realEstates = await _service.GetAllRealEstatesAsync();
                _logger.LogInformation("Real estate list retrieved successfully");
                return Ok(new ResponseModel<IEnumerable<RealEstate>>
                {
                    StatusCode = 200,
                    Data = realEstates,
                    Message = "Real estate list retrieved successfully",
                    Exception = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving real estate list");
                return BadRequest(new ResponseModel<object>
                {
                    StatusCode = 400,
                    Data = null,
                    Message = "Error retrieving real estate list",
                    Exception = ex.Message
                });
            }
        }

        private readonly RealEstateCrudService _service;
        private readonly ILogger<RealEstateController> _logger;
    }
}