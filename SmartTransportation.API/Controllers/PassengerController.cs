using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTransportation.BLL.DTOs.Profile;
using SmartTransportation.BLL.Interfaces;
using System.Threading.Tasks;

namespace SmartTransportation.Web.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PassengerController : BaseApiController
    {
        private readonly IUserProfileService _profileService;

        public PassengerController(IUserProfileService profileService)
        {
            _profileService = profileService;
        }

        // -------------------------------
        // GET: Current passenger profile
        // -------------------------------
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            if (CurrentUserId is null)
                return Unauthorized("UserId claim missing in token.");

            var profile = await _profileService.GetByUserIdAsync(CurrentUserId.Value);

            // Return default empty profile instead of 404
            return Ok(profile ?? new BaseUserProfileDTO
            {
                FullName = "",
                Phone = "",
                Address = "",
                City = "",
                Country = "",
                Gender = "",
                DateOfBirth = null
            });
        }

        // -------------------------------
        // POST: Create passenger profile
        // -------------------------------
        [HttpPost("profile")]
        public async Task<IActionResult> CreateProfile([FromBody] CreateUserProfileDTO dto)
        {
            if (CurrentUserId is null)
                return Unauthorized("UserId claim missing in token.");

            var existing = await _profileService.GetByUserIdAsync(CurrentUserId.Value);
            if (existing != null)
                return BadRequest("Profile already exists. Use PUT to update.");

            var created = await _profileService.CreateAsync(dto, CurrentUserId.Value);
            return Ok(created);
        }

        // -------------------------------
        // PUT: Update passenger profile
        // Auto-create if profile doesn't exist
        // -------------------------------
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileDTO dto)
        {
            if (CurrentUserId is null)
                return Unauthorized("UserId claim missing in token.");

            var existing = await _profileService.GetByUserIdAsync(CurrentUserId.Value);

            // Auto-create if missing
            if (existing == null)
            {
                var createDto = new CreateUserProfileDTO
                {
                    FullName = dto.FullName ?? "",
                    Phone = dto.Phone,
                    Address = dto.Address,
                    City = dto.City,
                    Country = dto.Country,
                    DateOfBirth = dto.DateOfBirth,
                    Gender = dto.Gender
                };

                var created = await _profileService.CreateAsync(createDto, CurrentUserId.Value);
                return Ok(created);
            }

            // Update normal
            var updated = await _profileService.UpdateAsync(CurrentUserId.Value, dto);
            return Ok(updated);
        }
    }
}
