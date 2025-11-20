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

        // ---------------------------
        // Driver Profile Endpoints
        // ---------------------------
        [HttpGet("{driverId}")]
        public async Task<IActionResult> GetDriverFull(int driverId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null) return Unauthorized("UserId claim missing in token.");
            if (currentUserId != driverId) return Forbid();

            var driverFull = await _driverService.GetDriverFullByIdAsync(driverId);
            if (driverFull == null) return NotFound();

            return Ok(driverFull);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDriver([FromBody] CreateDriverProfileDTO dto)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized("UserId claim missing in token.");

            var result = await _driverService.CreateDriverAsync(dto, currentUserId.Value);
            return Ok(result);
        }

        [HttpPut("{driverId}")]
        public async Task<IActionResult> UpdateDriver(int driverId, [FromBody] UpdateDriverProfileDTO dto)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null) return Unauthorized("UserId claim missing in token.");
            if (currentUserId != driverId) return Forbid();

            var updated = await _driverService.UpdateDriverAsync(driverId, dto);
            if (updated == null) return NotFound();

            return Ok(updated);
        }
        // ---------------------------
        // Vehicle Endpoints
        // ---------------------------
        [HttpPost("vehicle")]
        public async Task<IActionResult> CreateVehicle([FromBody] CreateVehicleDTO dto)
        {
            var driverId = GetCurrentUserId();
            if (driverId == null)
                return Unauthorized("UserId claim missing in token.");

            var result = await _vehicleService.CreateVehicleAsync(driverId.Value, dto);
            return Ok(result);
        }

        [HttpPut("vehicle")]
        public async Task<IActionResult> UpdateVehicle([FromBody] UpdateVehicleDTO dto)
        {
            var driverId = GetCurrentUserId();
            if (driverId == null)
                return Unauthorized("UserId claim missing in token.");

            // Optional: verify vehicle belongs to current driver
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
            if (driverId == null) return Unauthorized("UserId claim missing in token.");

            var vehicle = await _vehicleService.GetVehicleByDriverIdAsync(driverId.Value);
            if (vehicle == null) return NotFound();

            return Ok(vehicle);
        }

    }
}
