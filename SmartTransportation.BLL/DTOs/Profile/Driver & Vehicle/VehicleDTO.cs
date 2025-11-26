using System;

namespace SmartTransportation.BLL.DTOs.Profile
{
    /// <summary>
    /// DTO for vehicle information
    /// Used for displaying and transferring vehicle data
    /// </summary>
    public class VehicleDTO
    {
        /// <summary>
        /// Unique vehicle identifier
        /// </summary>
        public int VehicleId { get; set; }

        /// <summary>
        /// ID of the driver who owns this vehicle
        /// </summary>
        public int DriverId { get; set; }

        /// <summary>
        /// Vehicle manufacturer (e.g., Toyota, Honda)
        /// </summary>
        public string VehicleMake { get; set; }

        /// <summary>
        /// Vehicle model (e.g., Camry, Civic)
        /// </summary>
        public string VehicleModel { get; set; }

        /// <summary>
        /// Manufacturing year of the vehicle
        /// </summary>
        public int? VehicleYear { get; set; }

        /// <summary>
        /// License plate number
        /// </summary>
        public string PlateNumber { get; set; }

        /// <summary>
        /// Vehicle color
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Number of passenger seats available
        /// ⚠️ Changed from int to int? to match database entity
        /// </summary>
        public int? SeatsCount { get; set; }

        /// <summary>
        /// Vehicle license/registration number
        /// </summary>
        public string VehicleLicenseNumber { get; set; }

        /// <summary>
        /// Vehicle license expiry date
        /// ⚠️ Changed from DateOnly? to DateTime? to match database entity
        /// </summary>
        public DateTime? VehicleLicenseExpiry { get; set; }

        /// <summary>
        /// Whether the vehicle has been verified by admin
        /// </summary>
        public bool IsVerified { get; set; }
    }
}