using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartTransportation.BLL.DTOs.Profile;
using SmartTransportation.BLL.Interfaces;
using System;
using System.Threading.Tasks;

namespace SmartTransportation.Web.Pages
{
    [Authorize(Roles = "Driver")]
    public class Driver_ProfileModel : PageModel
    {
        private readonly IDriverService _driverService;
        private readonly IVehicleService _vehicleService;

        public Driver_ProfileModel(IDriverService driverService, IVehicleService vehicleService)
        {
            _driverService = driverService;
            _vehicleService = vehicleService;
        }

        // -------------------------
        // Driver Properties
        // -------------------------
        [BindProperty]
        public string FullName { get; set; } = "";

        [BindProperty]
        public string? Phone { get; set; }

        [BindProperty]
        public string? Address { get; set; }

        [BindProperty]
        public string? City { get; set; }

        [BindProperty]
        public string? Country { get; set; }

        [BindProperty]
        public DateOnly? DateOfBirth { get; set; }

        [BindProperty]
        public string? Gender { get; set; }

        [BindProperty]
        public string? ProfilePhotoUrl { get; set; }

        [BindProperty]
        public string? DriverLicenseNumber { get; set; }

        [BindProperty]
        public DateOnly? DriverLicenseExpiry { get; set; }

        [BindProperty]
        public decimal? DriverRating { get; set; }

        [BindProperty]
        public bool IsDriverVerified { get; set; }

        // -------------------------
        // Vehicle Properties
        // -------------------------
        [BindProperty]
        public int VehicleId { get; set; }

        [BindProperty]
        public string? VehicleMake { get; set; }

        [BindProperty]
        public string? VehicleModel { get; set; }

        [BindProperty]
        public int? VehicleYear { get; set; }

        [BindProperty]
        public string? PlateNumber { get; set; }

        [BindProperty]
        public string? Color { get; set; }

        [BindProperty]
        public int SeatsCount { get; set; }

        [BindProperty]
        public string? VehicleLicenseNumber { get; set; }

        [BindProperty]
        public DateOnly? VehicleLicenseExpiry { get; set; }

        [BindProperty]
        public bool VehicleIsVerified { get; set; }

        // -------------------------
        // Stats (optional: can be fetched dynamically from service)
        // -------------------------
        public int TotalTrips { get; set; }
        public int TotalMiles { get; set; }
        public int SafetyScore { get; set; }

        public string? SuccessMessage { get; set; }

        // -------------------------
        // Helper: get current driver ID from JWT
        // -------------------------
        private int GetCurrentDriverId()
        {
            var claim = User.FindFirst("UserId") ?? User.FindFirst("UserID");
            if (claim == null) throw new Exception("UserId claim missing in token.");
            return int.Parse(claim.Value);
        }

        // -------------------------
        // GET: Load driver & vehicle info dynamically
        // -------------------------
        public async Task OnGetAsync()
        {
            int driverId = GetCurrentDriverId();

            // Fetch full driver info including vehicle
            DriverFullDTO driverFull = await _driverService.GetDriverFullByIdAsync(driverId);
            if (driverFull?.Driver != null)
            {
                var driver = driverFull.Driver;

                // Map driver fields
                FullName = driver.FullName ?? "";
                Phone = driver.Phone;
                Address = driver.Address;
                City = driver.City;
                Country = driver.Country;
                DateOfBirth = driver.DateOfBirth;
                Gender = driver.Gender;
                ProfilePhotoUrl = driver.ProfilePhotoUrl;
                DriverLicenseNumber = driver.DriverLicenseNumber;
                DriverLicenseExpiry = driver.DriverLicenseExpiry;
                DriverRating = driver.DriverRating;
                IsDriverVerified = driver.IsDriverVerified;
            }

            if (driverFull?.Vehicle != null)
            {
                var vehicle = driverFull.Vehicle;

                // Map vehicle fields
                VehicleId = vehicle.VehicleId;
                VehicleMake = vehicle.VehicleMake;
                VehicleModel = vehicle.VehicleModel;
                VehicleYear = vehicle.VehicleYear;
                PlateNumber = vehicle.PlateNumber;
                Color = vehicle.Color;
                SeatsCount = vehicle.SeatsCount;
                VehicleLicenseNumber = vehicle.VehicleLicenseNumber;
                VehicleLicenseExpiry = vehicle.VehicleLicenseExpiry;
                VehicleIsVerified = vehicle.IsVerified;
            }

            // Optional: fetch real stats dynamically from a service if available
            TotalTrips = 0; // replace with actual service call
            TotalMiles = 0; // replace with actual service call
            SafetyScore = 0; // replace with actual service call
        }

        // -------------------------
        // POST: Update driver profile
        // -------------------------
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            int driverId = GetCurrentDriverId();

            var updateDto = new UpdateDriverProfileDTO
            {
                FullName = FullName,
                Phone = Phone,
                Address = Address,
                City = City,
                Country = Country,
                DateOfBirth = DateOfBirth,
                Gender = Gender,
                ProfilePhotoUrl = ProfilePhotoUrl,
                DriverLicenseNumber = DriverLicenseNumber,
                DriverLicenseExpiry = DriverLicenseExpiry
            };

            var result = await _driverService.UpdateDriverAsync(driverId, updateDto);
            if (result != null) SuccessMessage = "Profile updated successfully!";

            return RedirectToPage(); // reload page with updated data
        }

        // -------------------------
        // POST: Update vehicle
        // -------------------------
        public async Task<IActionResult> OnPostUpdateVehicleAsync()
        {
            if (!ModelState.IsValid) return Page();

            int driverId = GetCurrentDriverId();

            var vehicleDto = new UpdateVehicleDTO
            {
                VehicleId = VehicleId,
                VehicleMake = VehicleMake,
                VehicleModel = VehicleModel,
                VehicleYear = VehicleYear,
                PlateNumber = PlateNumber,
                Color = Color,
                SeatsCount = SeatsCount,
                VehicleLicenseNumber = VehicleLicenseNumber,
                VehicleLicenseExpiry = VehicleLicenseExpiry
            };

            var result = await _vehicleService.UpdateVehicleAsync(VehicleId, vehicleDto);
            if (result != null) SuccessMessage = "Vehicle updated successfully!";

            return RedirectToPage(); // reload page with updated data
        }
    }
}
