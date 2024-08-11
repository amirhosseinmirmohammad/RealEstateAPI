using RealEstateCore.Enums;

namespace RealEstateCore.Models
{
    /// <summary>
    /// Represents a real estate property.
    /// </summary>
    public class RealEstate : BaseModel
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
        /// The current status of the real estate property.
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

        /// <summary>
        /// The list of photos associated with the real estate property.
        /// </summary>
        public virtual List<RealEstatePhoto> Photos { get; set; } = new List<RealEstatePhoto>();

        /// <summary>
        /// The list of change logs associated with the real estate property.
        /// </summary>
        public virtual List<ChangeLog> ChangeLogs { get; set; } = new List<ChangeLog>();
    }
}
