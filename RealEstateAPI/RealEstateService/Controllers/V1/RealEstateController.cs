using Microsoft.AspNetCore.Mvc;
using RealEstateApplication.Services.V1;
using RealEstateCore.Models;
using RealEstateService.ViewModels;

namespace RealEstateService.Controllers.V1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class RealEstateController : ControllerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RealEstateController"/> class.
        /// </summary>
        /// <param name="realEstateService">The service for managing real estate operations.</param>
        public RealEstateController(RealEstatesService realEstateService)
        {
            _realEstateService = realEstateService;
        }

        [HttpGet("similar")]
        public async Task<ActionResult<IEnumerable<RealEstate>>> GetSimilarTitles([FromQuery] string input)
        {
            ResponseModel<IEnumerable<RealEstate>> result = await _realEstateService.GetSimilarTitlesAsync(input);

            return Ok(result);
        }

        /// <summary>
        /// Adds a new real estate property.
        /// </summary>
        /// <param name="realEstateViewModel">The view model containing the details of the real estate property.</param>
        /// <returns>A <see cref="ResponseModel{T}"/> containing the ID of the newly added real estate property.</returns>
        [HttpPost("add")]
        public async Task<ResponseModel<int>> AddRealEstate([FromBody] RealEstateViewModel realEstateViewModel)
        {
            return await _realEstateService.AddRealEstateAsync(realEstateViewModel);
        }

        /// <summary>
        /// Archives an existing real estate property by its ID.
        /// </summary>
        /// <param name="id">The ID of the real estate property to archive.</param>
        /// <returns>A <see cref="ResponseModel{T}"/> indicating the status of the operation.</returns>
        [HttpPut("archive/{id}")]
        public async Task<ResponseModel<int>> ArchiveRealEstate(int id)
        {
            return await _realEstateService.ArchiveRealEstateAsync(id);
        }

        /// <summary>
        /// Updates the timestamp of an existing real estate property.
        /// </summary>
        /// <param name="id">The ID of the real estate property to update.</param>
        /// <returns>A <see cref="ResponseModel{T}"/> containing the ID of the updated real estate property.</returns>
        [HttpPut("update-time/{id}")]
        public async Task<ResponseModel<int>> UpdateRealEstateTime(int id)
        {
            return await _realEstateService.UpdateRealEstateTimeAsync(id);
        }

        /// <summary>
        /// Retrieves a list of all real estate properties.
        /// </summary>
        /// <returns>A <see cref="ResponseModel{T}"/> containing a list of all real estate properties.</returns>
        [HttpGet("list")]
        public async Task<ResponseModel<IEnumerable<RealEstate>>> GetAllRealEstates()
        {
            return await _realEstateService.GetAllRealEstatesAsync();
        }

        private readonly RealEstatesService _realEstateService;
    }
}
