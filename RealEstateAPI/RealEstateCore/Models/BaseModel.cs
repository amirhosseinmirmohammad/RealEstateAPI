namespace RealEstateCore.Models
{
    /// <summary>
    /// Base class for all models, including common properties such as CreatedAt and UpdatedAt.
    /// </summary>
    public class BaseModel
    {
        /// <summary>
        /// The date and time when the entity was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The date and time when the entity was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// The ID of the user who created or updated the entity.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseModel"/> class.
        /// </summary>
        public BaseModel()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}