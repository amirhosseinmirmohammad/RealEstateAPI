using RealEstateCore.Enums;

namespace RealEstateCore.Models
{
    public class RealEstate : BaseModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public RealEstateStatus Status { get; set; }
        public decimal Price { get; set; }
        public int Floor { get; set; }
        public virtual List<RealEstatePhoto> Photos { get; set; } = new List<RealEstatePhoto>();
        public virtual List<ChangeLog> ChangeLogs { get; set; } = new List<ChangeLog>();
    }
}
