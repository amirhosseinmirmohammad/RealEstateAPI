using Microsoft.Extensions.Logging;
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

        public async Task<ResponseModel<List<string>>> AddPhotosToRealEstateAsync(int realEstateId, List<string> filePaths)
        {
            try
            {
                var realEstate = await _repository.GetRealEstateByIdAsync(realEstateId);
                if (realEstate != null)
                {
                    var uploadedPhotoUrls = new List<string>();

                    foreach (var filePath in filePaths)
                    {
                        if (File.Exists(filePath))
                        {
                            var fileName = Path.GetFileName(filePath);
                            var destinationPath = Path.Combine("wwwroot/images", fileName);
                            File.Copy(filePath, destinationPath, overwrite: true);

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
                            _logger.LogWarning("File not found: {FilePath}", filePath);
                        }
                    }

                    await _repository.UpdateRealEstateAsync(realEstate);
                    _logger.LogInformation("Photos added to RealEstate with ID {RealEstateId}", realEstateId);

                    return new ResponseModel<List<string>>
                    {
                        StatusCode = 200,
                        Data = uploadedPhotoUrls,
                        Message = "Photos uploaded successfully"
                    };
                }

                return new ResponseModel<List<string>>
                {
                    StatusCode = 404,
                    Data = null,
                    Message = "Real estate not found"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding photos to real estate");
                return new ResponseModel<List<string>>
                {
                    StatusCode = 500,
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
                            StatusCode = 200,
                            Data = photoId,
                            Message = "Photo deleted successfully"
                        };
                    }

                    return new ResponseModel<int>
                    {
                        StatusCode = 404,
                        Data = 0,
                        Message = "Photo not found"
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
                _logger.LogError(ex, "Error deleting photo from real estate");
                return new ResponseModel<int>
                {
                    StatusCode = 500,
                    Data = 0,
                    Message = "Error deleting photo from real estate",
                    Exception = ex.Message
                };
            }
        }

        public async Task<ResponseModel<int>> EditPhotoInRealEstateAsync(int realEstateId, int photoId, string newPhotoPath)
        {
            try
            {
                var realEstate = await _repository.GetRealEstateByIdAsync(realEstateId);
                if (realEstate != null)
                {
                    var photo = realEstate.Photos.FirstOrDefault(p => p.Id == photoId);
                    if (photo != null)
                    {
                        if (File.Exists(newPhotoPath))
                        {
                            var fileName = Path.GetFileName(newPhotoPath);
                            var destinationPath = Path.Combine("wwwroot/images", fileName);
                            File.Copy(newPhotoPath, destinationPath, overwrite: true);

                            var newPhotoUrl = $"/images/{fileName}";
                            photo.PhotoUrl = newPhotoUrl;
                            await _repository.UpdateRealEstateAsync(realEstate);
                            _logger.LogInformation("Photo with ID {PhotoId} updated in RealEstate with ID {RealEstateId}", photoId, realEstateId);

                            return new ResponseModel<int>
                            {
                                StatusCode = 200,
                                Data = photoId,
                                Message = "Photo updated successfully"
                            };
                        }

                        return new ResponseModel<int>
                        {
                            StatusCode = 404,
                            Data = 0,
                            Message = "New photo file not found"
                        };
                    }

                    return new ResponseModel<int>
                    {
                        StatusCode = 404,
                        Data = 0,
                        Message = "Photo not found"
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
                _logger.LogError(ex, "Error updating photo in real estate");
                return new ResponseModel<int>
                {
                    StatusCode = 500,
                    Data = 0,
                    Message = "Error updating photo in real estate",
                    Exception = ex.Message
                };
            }
        }

        private readonly IRealEstateRepository _repository;
        private readonly ILogger<PhotosService> _logger;
    }
}
