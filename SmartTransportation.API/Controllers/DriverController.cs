using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTransportation.BLL.DTOs.Driver;
using SmartTransportation.BLL.DTOs.Profile;
using SmartTransportation.BLL.Interfaces;
using System.Threading.Tasks;

namespace SmartTransportation.Web.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DriverController : BaseApiController
    {
        private readonly IDriverService _driverService;
        private readonly IVehicleService _vehicleService;

        public DriverController(IDriverService driverService, IVehicleService vehicleService)
        {
            _driverService = driverService;
            _vehicleService = vehicleService;
        }

        private IActionResult UnauthorizedIfNoUserId()
        {
            if (CurrentUserId == null)
                return Unauthorized("UserId claim missing in token.");
            return null;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var unauthorized = UnauthorizedIfNoUserId();
            if (unauthorized != null) return unauthorized;

            var dashboard = await _driverService.GetDashboardAsync(CurrentUserId!.Value);
            return Ok(dashboard);
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetCurrentDriverProfile()
        {
            var unauthorized = UnauthorizedIfNoUserId();
            if (unauthorized != null) return unauthorized;

            var profile = await _driverService.GetDriverFullByIdAsync(CurrentUserId.Value);

            if (profile == null)
            {
                // Return default empty profile if not exists
                return Ok(new DriverFullDTO
                {
                    Driver = new DriverProfileDTO
                    {
                        FullName = "",
                        Phone = "",
                        Address = "",
                        City = "",
                        Country = "",
                        DateOfBirth = null,
                        Gender = "",
                        ProfilePhotoUrl = "",
                        DriverLicenseNumber = "",
                        DriverLicenseExpiry = null,
                        IsDriverVerified = false
                    },
                    Vehicle = null
                });
            }

            return Ok(profile);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateCurrentDriverProfile([FromBody] UpdateDriverProfileDTO dto)
        {
            var unauthorized = UnauthorizedIfNoUserId();
            if (unauthorized != null) return unauthorized;

            var updated = await _driverService.UpdateDriverAsync(CurrentUserId.Value, dto);

            // Auto-create if profile missing
            if (updated == null)
            {
                var createDto = new CreateDriverProfileDTO
                {
                    FullName = dto.FullName ?? "",
                    Phone = dto.Phone,
                    Address = dto.Address,
                    City = dto.City,
                    Country = dto.Country,
                    DateOfBirth = dto.DateOfBirth,
                    Gender = dto.Gender,
                    ProfilePhotoUrl = dto.ProfilePhotoUrl,
                    DriverLicenseNumber = dto.DriverLicenseNumber,
                    DriverLicenseExpiry = dto.DriverLicenseExpiry
                };

                var created = await _driverService.CreateDriverAsync(createDto, CurrentUserId.Value);
                return Ok(created);
            }

            return Ok(updated);
        }

        [HttpGet("vehicle")]
        public async Task<IActionResult> GetVehicle()
        {
            var unauthorized = UnauthorizedIfNoUserId();
            if (unauthorized != null) return unauthorized;

            var vehicle = await _vehicleService.GetVehicleByDriverIdAsync(CurrentUserId.Value);

            if (vehicle == null)
            {
                // Return default vehicle
                return Ok(new VehicleDTO
                {
                    VehicleId = 0,
                    DriverId = CurrentUserId.Value,
                    VehicleMake = "",
                    VehicleModel = "",
                    VehicleYear = null,
                    PlateNumber = "",
                    Color = "",
                    SeatsCount = 0,
                    VehicleLicenseNumber = "",
                    VehicleLicenseExpiry = null,
                    IsVerified = false
                });
            }

            return Ok(vehicle);
        }

        [HttpPut("vehicle")]
        public async Task<IActionResult> UpdateVehicle([FromBody] UpdateVehicleDTO dto)
        {
            var unauthorized = UnauthorizedIfNoUserId();
            if (unauthorized != null) return unauthorized;

            var existing = await _vehicleService.GetVehicleByDriverIdAsync(CurrentUserId.Value);

            if (existing == null)
            {
                // Auto-create vehicle if missing
                var createDto = new CreateVehicleDTO
                {
                    VehicleMake = dto.VehicleMake,
                    VehicleModel = dto.VehicleModel,
                    VehicleYear = dto.VehicleYear,
                    PlateNumber = dto.PlateNumber,
                    Color = dto.Color,
                    SeatsCount = dto.SeatsCount,
                    VehicleLicenseNumber = dto.VehicleLicenseNumber,
                    VehicleLicenseExpiry = dto.VehicleLicenseExpiry
                };

                var created = await _vehicleService.CreateVehicleAsync(CurrentUserId.Value, createDto);
                return Ok(created);
            }

            var updated = await _vehicleService.UpdateVehicleAsync(existing.VehicleId, dto);
            return Ok(updated);
        }

        [HttpPost("vehicle")]
        public async Task<IActionResult> CreateVehicle([FromBody] CreateVehicleDTO dto)
        {
            var unauthorized = UnauthorizedIfNoUserId();
            if (unauthorized != null) return unauthorized;

            // Always bind vehicle to the logged-in driver
            var result = await _vehicleService.CreateVehicleAsync(CurrentUserId.Value, dto);

            return Ok(result);
        }

    }
}
