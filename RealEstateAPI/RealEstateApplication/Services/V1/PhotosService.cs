using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RealEstateCore.Enums;
using RealEstateCore.Interfaces.V1;
using RealEstateCore.Models;

namespace RealEstateApplication.Services.V1
{
    public class PhotosService
    {
        public PhotosService(IRealEstateRepository repository,
                                ILogger<PhotosService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ResponseModel<List<string>>> AddPhotosToRealEstateAsync(int realEstateId, List<IFormFile> files)
        {
            try
            {
                var realEstate = await _repository.GetRealEstateByIdAsync(realEstateId);
                if (realEstate != null)
                {
                    var uploadedPhotoUrls = new List<string>();

                    foreach (var file in files)
                    {
                        if (file.Length > 0)
                        {
                            var fileName = Path.GetFileName(file.FileName);
                            var destinationPath = Path.Combine("wwwroot/images", fileName);
                            using (var stream = new FileStream(destinationPath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            var photoUrl = $"/images/{fileName}";
                            uploadedPhotoUrls.Add(photoUrl);

                            realEstate.Photos.Add(new RealEstatePhoto
                            {
                                PhotoUrl = photoUrl,
                                RealEstateId = realEstateId
                            });
                        }
                        else
                        {
                            _logger.LogWarning("File is empty: {FileName}", file.FileName);
                        }
                    }

                    await _repository.UpdateRealEstateAsync(realEstate);
                    _logger.LogInformation("Photos added to RealEstate with ID {RealEstateId}", realEstateId);

                    return new ResponseModel<List<string>>
                    {
                        StatusCode = (int)ResponseStatus.Success,
                        Data = uploadedPhotoUrls,
                        Message = "Photos uploaded successfully"
                    };
                }

                return new ResponseModel<List<string>>
                {
                    StatusCode = (int)ResponseStatus.NotFound,
                    Data = null,
                    Message = "Real estate not found"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding photos to real estate");
                return new ResponseModel<List<string>>
                {
                    StatusCode = (int)ResponseStatus.ServerError,
                    Data = null,
                    Message = "Error adding photos to real estate",
                    Exception = ex.Message
                };
            }
        }

        public async Task<ResponseModel<int>> DeletePhotoFromRealEstateAsync(int realEstateId, int photoId)
        {
            try
            {
                var realEstate = await _repository.GetRealEstateByIdAsync(realEstateId);
                if (realEstate != null)
                {
                    var photo = realEstate.Photos.FirstOrDefault(p => p.Id == photoId);
                    if (photo != null)
                    {
                        var filePath = Path.Combine("wwwroot/images", Path.GetFileName(photo.PhotoUrl));
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }

                        realEstate.Photos.Remove(photo);
                        await _repository.UpdateRealEstateAsync(realEstate);
                        _logger.LogInformation("Photo deleted from RealEstate with ID {RealEstateId}", realEstateId);

                        return new ResponseModel<int>
                        {
                            StatusCode = (int)ResponseStatus.Success,
                            Data = photoId,
                            Message = "Photo deleted successfully"
                        };
                    }

                    return new ResponseModel<int>
                    {
                        StatusCode = (int)ResponseStatus.NotFound,
                        Data = 0,
                        Message = "Photo not found"
                    };
                }

                return new ResponseModel<int>
                {
                    StatusCode = (int)ResponseStatus.NotFound,
                    Data = 0,
                    Message = "Real estate not found"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting photo from real estate");
                return new ResponseModel<int>
                {
                    StatusCode = (int)ResponseStatus.ServerError,
                    Data = 0,
                    Message = "Error deleting photo from real estate",
                    Exception = ex.Message
                };
            }
        }

        private readonly IRealEstateRepository _repository;
        private readonly ILogger<PhotosService> _logger;
    }
}
