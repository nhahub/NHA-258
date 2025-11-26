using System;

namespace SmartTransportation.BLL.DTOs.Profile
{
    /// <summary>
    /// DTO for creating a new vehicle
    /// Used when a driver registers their vehicle
    /// </summary>
    public class CreateVehicleDTO
    {
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
    }
}