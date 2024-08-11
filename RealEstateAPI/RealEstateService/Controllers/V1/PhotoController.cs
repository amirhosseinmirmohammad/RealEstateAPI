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
        /// <param name="files">List of photo files to upload.</param>
        /// <returns>A ResponseModel containing the URLs of the uploaded photos.</returns>
        [HttpPost("upload/{realEstateId}")]
        public async Task<ResponseModel<List<string>>> UploadPhotos(int realEstateId, [FromForm] List<IFormFile> files)
        {
            return await _photoService.AddPhotosToRealEstateAsync(realEstateId, files);
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

        private readonly PhotosService _photoService;
    }
}
