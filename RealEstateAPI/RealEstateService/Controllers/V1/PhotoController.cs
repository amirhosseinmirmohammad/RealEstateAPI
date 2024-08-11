using Microsoft.AspNetCore.Mvc;
using RealEstateApplication.Services.V1;
using RealEstateCore.Models;

namespace RealEstateService.Controllers.V1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PhotoController : ControllerBase
    {
        public PhotoController(PhotosService photoService)
        {
            _photoService = photoService;
        }

        /// <summary>
        /// Upload multiple photos for a specific real estate property.
        /// </summary>
        /// <param name="realEstateId">The ID of the real estate.</param>
        /// <param name="filePaths">List of file paths to upload.</param>
        /// <returns>A ResponseModel containing the URLs of the uploaded photos.</returns>
        [HttpPost("upload/{realEstateId}")]
        public async Task<ResponseModel<List<string>>> UploadPhotos(int realEstateId, [FromBody] List<string> filePaths)
        {
            return await _photoService.AddPhotosToRealEstateAsync(realEstateId, filePaths);
        }

        /// <summary>
        /// Delete a specific photo from a real estate property.
        /// </summary>
        /// <param name="realEstateId">The ID of the real estate.</param>
        /// <param name="photoId">The ID of the photo to delete.</param>
        /// <returns>A ResponseModel containing the ID of the deleted photo.</returns>
        [HttpDelete("delete/{realEstateId}/{photoId}")]
        public async Task<ResponseModel<int>> DeletePhoto(int realEstateId, int photoId)
        {
            return await _photoService.DeletePhotoFromRealEstateAsync(realEstateId, photoId);
        }

        /// <summary>
        /// Edit the URL of a specific photo for a real estate property.
        /// </summary>
        /// <param name="realEstateId">The ID of the real estate.</param>
        /// <param name="photoId">The ID of the photo to edit.</param>
        /// <param name="newPhotoPath">The new file path for the photo.</param>
        /// <returns>A ResponseModel containing the ID of the updated photo.</returns>
        [HttpPut("edit/{realEstateId}/{photoId}")]
        public async Task<ResponseModel<int>> EditPhoto(int realEstateId, int photoId, [FromBody] string newPhotoPath)
        {
            return await _photoService.EditPhotoInRealEstateAsync(realEstateId, photoId, newPhotoPath);
        }

        private readonly PhotosService _photoService;
    }
}
