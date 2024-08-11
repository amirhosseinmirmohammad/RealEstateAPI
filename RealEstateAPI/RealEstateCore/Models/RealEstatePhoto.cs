using System.Text.Json.Serialization;

namespace RealEstateCore.Models
{
    /// <summary>
    /// Represents a photo associated with a real estate property.
    /// </summary>
    public class RealEstatePhoto
    {
        /// <summary>
        /// The unique identifier of the real estate photo.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The URL of the photo.
        /// </summary>
        public string PhotoUrl { get; set; }

        /// <summary>
        /// The ID of the associated real estate property.
        /// </summary>
        public int RealEstateId { get; set; }

        /// <summary>
        /// The associated real estate property.
        /// </summary>
        [JsonIgnore]
        public virtual RealEstate RealEstate { get; set; }
    }
}