using System;

namespace SmartTransportation.BLL.DTOs.Profile
{
    /// <summary>
    /// DTO for driver profile information
    /// Inherits base user properties from BaseUserProfileDTO
    /// </summary>
    public class DriverProfileDTO : BaseUserProfileDTO
    {
        // ✅ ONLY properties that are NOT in BaseUserProfileDTO

        /// <summary>
        /// User ID from the Users table (required for frontend operations)
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Driver's license number
        /// </summary>
        public string DriverLicenseNumber { get; set; }

        /// <summary>
        /// Driver's license expiry date
        /// ⚠️ Changed from DateOnly? to DateTime? to match database entity
        /// </summary>
        public DateTime? DriverLicenseExpiry { get; set; }

        /// <summary>
        /// Driver's average rating
        /// ⚠️ Changed from decimal? to double? to match database entity
        /// </summary>
        public double? DriverRating { get; set; }

        /// <summary>
        /// Whether the driver has been verified by admin
        /// </summary>
        public bool IsDriverVerified { get; set; }
    }
}