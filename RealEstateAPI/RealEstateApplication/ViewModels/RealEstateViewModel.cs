using RealEstateCore.Enums;

namespace RealEstateApplication.ViewModels
{
    /// <summary>
    /// ViewModel for representing real estate properties.
    /// </summary>
    public class RealEstateViewModel
    {
        /// <summary>
        /// The unique identifier of the real estate property.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The title or name of the real estate property.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The status of the real estate property.
        /// </summary>
        public RealEstateStatus Status { get; set; }

        /// <summary>
        /// The price of the real estate property.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// The floor number of the real estate property (if applicable).
        /// </summary>
        public int Floor { get; set; }
    }
}
