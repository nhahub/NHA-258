using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTransportation.BLL.DTOs.Profile;
using SmartTransportation.BLL.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace SmartTransportation.Web.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        private int? GetCurrentAdminId()
        {
            var claim = User.FindFirst("UserId") ?? User.FindFirst("UserID");
            return claim != null ? int.Parse(claim.Value) : (int?)null;
        }

        // -------------------------------
        // Driver verification
        // -------------------------------
        [HttpPost("driver/verify")]
        public async Task<IActionResult> VerifyDriver([FromBody] VerifyDriverDTO dto)
        {
            var success = await _adminService.VerifyDriverAsync(dto.DriverId, dto.IsDriverVerified);
            return success ? Ok("Driver verified") : NotFound("Driver not found");
        }

        [HttpGet("drivers")]
        public async Task<IActionResult> GetDrivers([FromQuery] bool? onlyVerified = null)
        {
            var drivers = await _adminService.GetAllDriversAsync(onlyVerified);
            return Ok(drivers);
        }

        // -------------------------------
        // Vehicle verification
        // -------------------------------
        // Fetch vehicles by selected driver
        [HttpGet("vehicles/by-driver/{driverId}")]
        public async Task<IActionResult> GetVehiclesByDriver(int driverId)
        {
            var vehicles = await _adminService.GetVehiclesByDriverAsync(driverId);
            if (vehicles == null || !vehicles.Any())
                return NotFound("No vehicles found for this driver.");

            return Ok(vehicles);
        }

        [HttpPost("vehicle/verify")]
        public async Task<IActionResult> VerifyVehicle([FromBody] VerifyVehicleDTO dto)
        {
            // 1️⃣ Fetch all vehicles for this driver
            var vehicles = await _adminService.GetVehiclesByDriverAsync(dto.DriverId);

            if (!vehicles.Any())
                return NotFound("No vehicle found for this driver.");

            var vehicle = vehicles.First(); // auto-assign the first vehicle

            // 2️⃣ Attempt to verify the vehicle
            var success = await _adminService.VerifyVehicleAsync(vehicle.VehicleId, dto.DriverId, dto.IsVerified);

            // 3️⃣ Handle possible failure
            if (!success)
            {
                // The service returns false if the driver is not verified
                return BadRequest("Driver must be verified before verifying vehicle.");
            }

            return Ok("Vehicle verified successfully.");
        }



        [HttpGet("vehicles")]
        public async Task<IActionResult> GetVehicles([FromQuery] bool? onlyVerified = null)
        {
            var vehicles = await _adminService.GetAllVehiclesAsync(onlyVerified);
            return Ok(vehicles);
        }

        // -------------------------------
        // Admin profile
        // -------------------------------
        [HttpPost("profile")]
        public async Task<IActionResult> CreateAdminProfile([FromBody] CreateUserProfileDTO dto)
        {
            var adminId = GetCurrentAdminId();
            if (adminId == null) return Unauthorized();

            var profile = await _adminService.CreateAdminProfileAsync(dto, adminId.Value);
            return Ok(profile);
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetAdminProfile()
        {
            var adminId = GetCurrentAdminId();
            if (adminId == null) return Unauthorized();

            var profile = await _adminService.GetAdminProfileAsync(adminId.Value);
            return Ok(profile);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateAdminProfile([FromBody] UpdateUserProfileDTO dto)
        {
            var adminId = GetCurrentAdminId();
            if (adminId == null) return Unauthorized();

            var profile = await _adminService.UpdateAdminProfileAsync(adminId.Value, dto);
            return Ok(profile);
        }
    }
}
