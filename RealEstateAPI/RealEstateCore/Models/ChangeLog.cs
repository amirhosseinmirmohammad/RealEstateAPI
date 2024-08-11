namespace RealEstateCore.Models
{
    /// <summary>
    /// Represents a log of changes made to a real estate property.
    /// </summary>
    public class ChangeLog
    {
        /// <summary>
        /// The unique identifier of the change log entry.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The date and time when the change was made.
        /// </summary>
        public DateTime ChangeDate { get; set; }

        /// <summary>
        /// A description of the change that was made.
        /// </summary>
        public string Description { get; set; }
    }
}
