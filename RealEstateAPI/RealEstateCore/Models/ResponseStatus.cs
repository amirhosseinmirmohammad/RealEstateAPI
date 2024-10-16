namespace RealEstateCore.Models
{
    /// <summary>
    /// Enum representing different status codes for API responses.
    /// </summary>
    public enum ResponseStatus
    {
        /// <summary>
        /// The request was successful.
        /// </summary>
        Success = 200,

        /// <summary>
        /// The requested resource was not found.
        /// </summary>
        NotFound = 404,

        /// <summary>
        /// A server error occurred.
        /// </summary>
        ServerError = 500,

        /// <summary>
        /// The request was invalid.
        /// </summary>
        BadRequest = 400,

        /// <summary>
        /// Unauthorized access.
        /// </summary>
        Unauthorized = 401
    }
}