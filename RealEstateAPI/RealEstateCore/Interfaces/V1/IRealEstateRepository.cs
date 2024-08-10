using RealEstateCore.Models;

namespace RealEstateCore.Interfaces.V1
{
    public interface IRealEstateRepository
    {
        Task<int> AddRealEstateAsync(RealEstate realEstate);
        Task ArchiveRealEstateAsync(int id);
        Task<RealEstate> GetRealEstateByIdAsync(int id);
        Task<IEnumerable<RealEstate>> GetAllRealEstatesAsync();
        Task UpdateRealEstateAsync(RealEstate realEstate);
    }
}
