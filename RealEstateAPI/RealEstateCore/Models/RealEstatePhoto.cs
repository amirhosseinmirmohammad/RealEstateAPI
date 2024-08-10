namespace RealEstateCore.Models
{
    public class RealEstatePhoto
    {
        public int Id { get; set; }
        public string PhotoUrl { get; set; }
        public int RealEstateId { get; set; }
        public virtual RealEstate RealEstate { get; set; }
    }
}
