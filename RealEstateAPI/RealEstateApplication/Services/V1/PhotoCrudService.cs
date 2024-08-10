using Microsoft.Extensions.Logging;
using RealEstateCore.Interfaces.V1;
using RealEstateCore.Models;

namespace RealEstateApplication.Services.V1
{
    public class PhotoCrudService
    {
        public PhotoCrudService(IRealEstateRepository repository, 
                                ILogger<PhotoCrudService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task AddPhotoToRealEstate(int realEstateId, string photoUrl)
        {
            var realEstate = await _repository.GetRealEstateByIdAsync(realEstateId);
            if (realEstate != null)
            {
                realEstate.Photos.Add(new RealEstatePhoto
                {
                    PhotoUrl = photoUrl,
                    RealEstateId = realEstateId
                });
                await _repository.UpdateRealEstateAsync(realEstate);
                _logger.LogInformation("Photo added to RealEstate with ID {RealEstateId}", realEstateId);
            }
            else
            {
                _logger.LogWarning("RealEstate with ID {RealEstateId} not found", realEstateId);
            }
        }

        public async Task DeletePhotoFromRealEstate(int realEstateId, int photoId)
        {
            var realEstate = await _repository.GetRealEstateByIdAsync(realEstateId);
            if (realEstate != null)
            {
                var photo = realEstate.Photos.FirstOrDefault(p => p.Id == photoId);
                if (photo != null)
                {
                    realEstate.Photos.Remove(photo);
                    await _repository.UpdateRealEstateAsync(realEstate);
                    _logger.LogInformation("Photo deleted from RealEstate with ID {RealEstateId}", realEstateId);
                }
                else
                {
                    _logger.LogWarning("Photo with ID {PhotoId} not found in RealEstate with ID {RealEstateId}", photoId, realEstateId);
                }
            }
            else
            {
                _logger.LogWarning("RealEstate with ID {RealEstateId} not found", realEstateId);
            }
        }

        public async Task EditPhotoInRealEstate(int realEstateId, int photoId, string newPhotoUrl)
        {
            var realEstate = await _repository.GetRealEstateByIdAsync(realEstateId);
            if (realEstate != null)
            {
                var photo = realEstate.Photos.FirstOrDefault(p => p.Id == photoId);
                if (photo != null)
                {
                    photo.PhotoUrl = newPhotoUrl;
                    await _repository.UpdateRealEstateAsync(realEstate);
                    _logger.LogInformation("Photo with ID {PhotoId} updated in RealEstate with ID {RealEstateId}", photoId, realEstateId);
                }
                else
                {
                    _logger.LogWarning("Photo with ID {PhotoId} not found in RealEstate with ID {RealEstateId}", photoId, realEstateId);
                }
            }
            else
            {
                _logger.LogWarning("RealEstate with ID {RealEstateId} not found", realEstateId);
            }
        }

        private readonly IRealEstateRepository _repository;
        private readonly ILogger<PhotoCrudService> _logger;
    }
}
