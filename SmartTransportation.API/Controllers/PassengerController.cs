using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTransportation.BLL.DTOs.Profile;
using SmartTransportation.BLL.Interfaces;
using System.Threading.Tasks;

namespace SmartTransportation.Web.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Passenger")]
    public class PassengerController : ControllerBase
    {
        private readonly IUserProfileService _profileService;

        public PassengerController(IUserProfileService profileService)
        {
            _profileService = profileService;
        }

        // -------------------------------
        // Helper: get current user ID from JWT Aim: Passenger
        // -------------------------------
        private int? GetCurrentUserId()
        {
            var claim = User.FindFirst("UserId") ?? User.FindFirst("UserID");
            return claim != null ? int.Parse(claim.Value) : (int?)null;
        }

        // -------------------------------
        // Create Passenger Profile
        // Route: POST api/Passenger/profile
        // -------------------------------
        [HttpPost("profile")]
        public async Task<IActionResult> CreateMyProfile([FromBody] CreateUserProfileDTO dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized("UserId claim missing in token.");

            // Check if profile already exists
            var exists = await _profileService.GetByUserIdAsync(userId.Value);
            if (exists != null)
                return BadRequest("Profile already exists. Use PUT to update.");

            var created = await _profileService.CreateAsync(dto, userId.Value);
            return Ok(created);
        }

        // -------------------------------
        // Get Passenger Profile
        // Route: GET api/Passenger/profile
        // -------------------------------
        [HttpGet("profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized("UserId claim missing in token.");

            var profile = await _profileService.GetByUserIdAsync(userId.Value);
            if (profile == null) return NotFound("Profile not found.");

            return Ok(profile);
        }

        // -------------------------------
        // Update Passenger Profile
        // Route: PUT api/Passenger/profile
        // -------------------------------
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateUserProfileDTO dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized("UserId claim missing in token.");

            var updated = await _profileService.UpdateAsync(userId.Value, dto);
            if (updated == null) return NotFound("Profile not found.");

            return Ok(updated);
        }
    }
}
