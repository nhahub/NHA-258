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
    [Authorize(Roles = "Driver")]
    public class DriverController : BaseApiController
    {
        private readonly IDriverService _driverService;
        private readonly IVehicleService _vehicleService;

        public DriverController(IDriverService driverService, IVehicleService vehicleService)
        {
            _driverService = driverService;
            _vehicleService = vehicleService;
        }


        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var unauthorized = UnauthorizedIfNoUserId();
            if (unauthorized != null) return unauthorized;

            var dashboard = await _driverService.GetDashboardAsync(CurrentUserId!.Value);

            return Ok(dashboard);
        }




        /// <summary>
        /// Helper method: returns Unauthorized if CurrentUserId is missing
        /// </summary>
        private IActionResult UnauthorizedIfNoUserId()
        {
            if (CurrentUserId == null)
                return Unauthorized("UserId claim missing in token.");
            return null;
        }

        // ---------------------------
        // GET: Current driver's full profile
        // ---------------------------
        [HttpGet("profile")]
        public async Task<IActionResult> GetCurrentDriverProfile()
        {
            var unauthorized = UnauthorizedIfNoUserId();
            if (unauthorized != null) return unauthorized;

            var driverFull = await _driverService.GetDriverFullByIdAsync(CurrentUserId.Value);
            if (driverFull == null) return NotFound();

            return Ok(driverFull);
        }

        // ---------------------------
        // GET: Driver full profile by ID (self only)
        // ---------------------------
        [HttpGet("{driverId}")]
        public async Task<IActionResult> GetDriverFull(int driverId)
        {
            var unauthorized = UnauthorizedIfNoUserId();
            if (unauthorized != null) return unauthorized;

            if (CurrentUserId != driverId) return Forbid();

            var driverFull = await _driverService.GetDriverFullByIdAsync(driverId);
            if (driverFull == null) return NotFound();

            return Ok(driverFull);
        }

        // ---------------------------
        // POST: Create driver (binds to current user)
        // ---------------------------
        [HttpPost]
        public async Task<IActionResult> CreateDriver([FromBody] CreateDriverProfileDTO dto)
        {
            var unauthorized = UnauthorizedIfNoUserId();
            if (unauthorized != null) return unauthorized;

            var result = await _driverService.CreateDriverAsync(dto, CurrentUserId.Value);
            return Ok(result);
        }

        // ---------------------------
        // PUT: Update current driver profile
        // ---------------------------
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateCurrentDriverProfile([FromBody] UpdateDriverProfileDTO dto)
        {
            var unauthorized = UnauthorizedIfNoUserId();
            if (unauthorized != null) return unauthorized;

            var updated = await _driverService.UpdateDriverAsync(CurrentUserId.Value, dto);
            if (updated == null) return NotFound();

            return Ok(updated);
        }

        // ---------------------------
        // POST: Create vehicle for current driver
        [HttpPost("vehicle")]
        public async Task<IActionResult> CreateVehicle([FromBody] CreateVehicleDTO dto)
        {
            var unauthorized = UnauthorizedIfNoUserId();
            if (unauthorized != null) return unauthorized;

            // Always bind vehicle to the logged-in driver
            var result = await _vehicleService.CreateVehicleAsync(CurrentUserId.Value, dto);

            return Ok(result);
        }

        [HttpPut("vehicle")]
        public async Task<IActionResult> UpdateVehicle([FromBody] UpdateVehicleDTO dto)
        {
            var unauthorized = UnauthorizedIfNoUserId();
            if (unauthorized != null) return unauthorized;

            // Get the vehicle assigned to current driver
            var vehicle = await _vehicleService.GetVehicleByDriverIdAsync(CurrentUserId.Value);
            if (vehicle == null)
                return NotFound(new { Error = "NotFound", Message = "Vehicle does not exist for this driver." });

            // Update only using DTO fields
            var updated = await _vehicleService.UpdateVehicleAsync(vehicle.VehicleId, dto);

            return Ok(updated);
        }


        // ---------------------------
        // GET: Get current driver's vehicle
        // ---------------------------
        [HttpGet("vehicle")]
        public async Task<IActionResult> GetVehicle()
        {
            var unauthorized = UnauthorizedIfNoUserId();
            if (unauthorized != null) return unauthorized;

            var vehicle = await _vehicleService.GetVehicleByDriverIdAsync(CurrentUserId.Value);
            if (vehicle == null)
                return NotFound(new { Error = "NotFound", Message = "No vehicle found for this driver." });

            return Ok(vehicle);
        }
    }
}
