namespace RealEstateService.ViewModels
{
    using RealEstateCore.Enums;

    namespace RealEstateAPI.ViewModels
    {
        public class RealEstateViewModel
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public RealEstateStatus Status { get; set; }
            public decimal Price { get; set; }
            public int Floor { get; set; }
        }
    }
}
