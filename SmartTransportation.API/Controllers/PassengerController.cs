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
    public class PassengerController : BaseApiController
    {
        private readonly IUserProfileService _profileService;

        public PassengerController(IUserProfileService profileService)
        {
            _profileService = profileService;
        }

        // -------------------------------
        // GET: Current passenger profile
        // Route: GET api/Passenger/profile
        // -------------------------------
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            if (CurrentUserId == null) return Unauthorized("UserId claim missing in token.");

            var profile = await _profileService.GetByUserIdAsync(CurrentUserId.Value);
            if (profile == null) return NotFound("Profile not found.");

            return Ok(profile);
        }

        // -------------------------------
        // POST: Create passenger profile
        // Route: POST api/Passenger/profile
        // -------------------------------
        [HttpPost("profile")]
        public async Task<IActionResult> CreateProfile([FromBody] CreateUserProfileDTO dto)
        {
            if (CurrentUserId == null) return Unauthorized("UserId claim missing in token.");

            var existing = await _profileService.GetByUserIdAsync(CurrentUserId.Value);
            if (existing != null)
                return BadRequest("Profile already exists. Use PUT to update.");

            var created = await _profileService.CreateAsync(dto, CurrentUserId.Value);
            return Ok(created);
        }

        // -------------------------------
        // PUT: Update passenger profile
        // Route: PUT api/Passenger/profile
        // -------------------------------
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileDTO dto)
        {
            if (CurrentUserId == null) return Unauthorized("UserId claim missing in token.");

            var updated = await _profileService.UpdateAsync(CurrentUserId.Value, dto);
            if (updated == null) return NotFound("Profile not found.");

            return Ok(updated);
        }
    }
}
