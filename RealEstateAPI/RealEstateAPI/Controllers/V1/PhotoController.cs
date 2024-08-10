using Microsoft.AspNetCore.Mvc;
using RealEstateApplication.Services.V1;
using RealEstateCore.Models;

namespace RealEstateService.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class PhotoController : ControllerBase
    {
        public PhotoController(PhotoCrudService photoService,
                               ILogger<PhotoController> logger)
        {
            _photoService = photoService;
            _logger = logger;
        }

        [HttpPost("upload/{realEstateId}")]
        public async Task<IActionResult> UploadPhoto(int realEstateId, IFormFile file)
        {
            _logger.LogInformation("UploadPhoto API called for RealEstateId: {RealEstateId}", realEstateId);
            try
            {
                var filePath = Path.Combine("wwwroot/images", file.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var photoUrl = $"/images/{file.FileName}";
                await _photoService.AddPhotoToRealEstate(realEstateId, photoUrl);

                return Ok(new ResponseModel<string>
                {
                    StatusCode = 200,
                    Data = photoUrl,
                    Message = "Photo uploaded successfully",
                    Exception = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading photo for RealEstateId: {RealEstateId}", realEstateId);
                return BadRequest(new ResponseModel<object>
                {
                    StatusCode = 400,
                    Data = null,
                    Message = "Error uploading photo",
                    Exception = ex.Message
                });
            }
        }

        [HttpDelete("delete/{realEstateId}/{photoId}")]
        public async Task<IActionResult> DeletePhoto(int realEstateId, int photoId)
        {
            _logger.LogInformation("DeletePhoto API called for RealEstateId: {RealEstateId}, PhotoId: {PhotoId}", realEstateId, photoId);
            try
            {
                await _photoService.DeletePhotoFromRealEstate(realEstateId, photoId);
                return Ok(new ResponseModel<int>
                {
                    StatusCode = 200,
                    Data = photoId,
                    Message = "Photo deleted successfully",
                    Exception = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting photo with ID {PhotoId} for RealEstateId: {RealEstateId}", photoId, realEstateId);
                return BadRequest(new ResponseModel<object>
                {
                    StatusCode = 400,
                    Data = null,
                    Message = "Error deleting photo",
                    Exception = ex.Message
                });
            }
        }

        [HttpPut("edit/{realEstateId}/{photoId}")]
        public async Task<IActionResult> EditPhoto(int realEstateId, int photoId, [FromBody] string newPhotoUrl)
        {
            _logger.LogInformation("EditPhoto API called for RealEstateId: {RealEstateId}, PhotoId: {PhotoId}", realEstateId, photoId);
            try
            {
                await _photoService.EditPhotoInRealEstate(realEstateId, photoId, newPhotoUrl);
                return Ok(new ResponseModel<int>
                {
                    StatusCode = 200,
                    Data = photoId,
                    Message = "Photo edited successfully",
                    Exception = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing photo with ID {PhotoId} for RealEstateId: {RealEstateId}", photoId, realEstateId);
                return BadRequest(new ResponseModel<object>
                {
                    StatusCode = 400,
                    Data = null,
                    Message = "Error editing photo",
                    Exception = ex.Message
                });
            }
        }

        private readonly PhotoCrudService _photoService;
        private readonly ILogger<PhotoController> _logger;
    }
}
