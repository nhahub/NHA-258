using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTransportation.BLL.DTOs.Profile;
using SmartTransportation.BLL.Interfaces;
using System.Threading.Tasks;

namespace SmartTransportation.Web.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Driver")]
    public class DriverController : ControllerBase
    {
        private readonly IDriverService _driverService;
        private readonly IVehicleService _vehicleService;

        public DriverController(IDriverService driverService, IVehicleService vehicleService)
        {
            _driverService = driverService;
            _vehicleService = vehicleService;
        }

        // ---------------------------
        // Helper: get current user ID from JWT
        // ---------------------------
        private int? GetCurrentUserId()
        {
            var claim = User.FindFirst("UserId") ?? User.FindFirst("UserID");
            return claim != null ? int.Parse(claim.Value) : (int?)null;
        }

        private IActionResult UnauthorizedIfNoUserId(int? userId)
        {
            if (userId == null) return Unauthorized("UserId claim missing in token.");
            return null;
        }

        // ---------------------------
        // Get current driver full profile
        // ---------------------------
        [HttpGet("profile")]
        public async Task<IActionResult> GetCurrentDriverProfile()
        {
            var driverId = GetCurrentUserId();
            var unauthorized = UnauthorizedIfNoUserId(driverId);
            if (unauthorized != null) return unauthorized;

            var driverFull = await _driverService.GetDriverFullByIdAsync(driverId.Value);
            if (driverFull == null) return NotFound();

            return Ok(driverFull);
        }

        // ---------------------------
        // Get driver by ID (still works)
        // ---------------------------
        [HttpGet("{driverId}")]
        public async Task<IActionResult> GetDriverFull(int driverId)
        {
            var currentUserId = GetCurrentUserId();
            var unauthorized = UnauthorizedIfNoUserId(currentUserId);
            if (unauthorized != null) return unauthorized;

            if (currentUserId != driverId) return Forbid();

            var driverFull = await _driverService.GetDriverFullByIdAsync(driverId);
            if (driverFull == null) return NotFound();

            return Ok(driverFull);
        }

        // ---------------------------
        // Create driver
        // ---------------------------
        [HttpPost]
        public async Task<IActionResult> CreateDriver([FromBody] CreateDriverProfileDTO dto)
        {
            var driverId = GetCurrentUserId();
            var unauthorized = UnauthorizedIfNoUserId(driverId);
            if (unauthorized != null) return unauthorized;

            var result = await _driverService.CreateDriverAsync(dto, driverId.Value);
            return Ok(result);
        }

        // ---------------------------
        // Update current driver profile
        // ---------------------------
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateCurrentDriverProfile([FromBody] UpdateDriverProfileDTO dto)
        {
            var driverId = GetCurrentUserId();
            var unauthorized = UnauthorizedIfNoUserId(driverId);
            if (unauthorized != null) return unauthorized;

            var updated = await _driverService.UpdateDriverAsync(driverId.Value, dto);
            if (updated == null) return NotFound();

            return Ok(updated);
        }

        // ---------------------------
        // Vehicle endpoints
        // ---------------------------
        [HttpPost("vehicle")]
        public async Task<IActionResult> CreateVehicle([FromBody] CreateVehicleDTO dto)
        {
            var driverId = GetCurrentUserId();
            var unauthorized = UnauthorizedIfNoUserId(driverId);
            if (unauthorized != null) return unauthorized;

            var result = await _vehicleService.CreateVehicleAsync(driverId.Value, dto);
            return Ok(result);
        }

        [HttpPut("vehicle")]
        public async Task<IActionResult> UpdateVehicle([FromBody] UpdateVehicleDTO dto)
        {
            var driverId = GetCurrentUserId();
            var unauthorized = UnauthorizedIfNoUserId(driverId);
            if (unauthorized != null) return unauthorized;

            // Verify vehicle belongs to current driver
            var vehicle = await _vehicleService.GetByIdAsync(dto.VehicleId);
            if (vehicle == null) return NotFound();
            if (vehicle.DriverId != driverId.Value) return Forbid();

            var result = await _vehicleService.UpdateVehicleAsync(dto.VehicleId, dto);
            return Ok(result);
        }

        [HttpGet("vehicle")]
        public async Task<IActionResult> GetVehicle()
        {
            var driverId = GetCurrentUserId();
            var unauthorized = UnauthorizedIfNoUserId(driverId);
            if (unauthorized != null) return unauthorized;

            var vehicle = await _vehicleService.GetVehicleByDriverIdAsync(driverId.Value);
            if (vehicle == null) return NotFound();

            return Ok(vehicle);
        }
    }
}
