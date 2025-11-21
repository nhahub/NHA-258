using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTransportation.BLL.DTOs.Trip;
using SmartTransportation.BLL.Interfaces;
using SmartTransportation.DAL.Models.Common;
using System.Security.Claims;
using System.Threading.Tasks;
using System;

namespace SmartTransportation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // كل الأوامر هنا للـ Driver فقط
    public class TripsController : ControllerBase
    {
        private readonly ITripService _tripService;

        public TripsController(ITripService tripService)
        {
            _tripService = tripService;
        }

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirst("UserId") ?? User.FindFirst("UserID") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null && int.TryParse(claim.Value, out int userId))
                return userId;
            return null;
        }

        [HttpPost]
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> CreateTrip([FromBody] CreateTripDTO tripDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Error = "BadRequest", Message = "Invalid trip data.", Details = ModelState });

            var driverId = GetCurrentUserId();
            if (driverId == null)
                return Unauthorized(new { Error = "Unauthorized", Message = "Invalid token or driver ID." });

            try
            {
                var newTrip = await _tripService.CreateTripAsync(tripDto, driverId.Value);
                return CreatedAtAction(nameof(GetTripById), new { id = newTrip.TripId }, newTrip);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = "Error", Message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTripById(int id)
        {
            var trip = await _tripService.GetTripDetailsByIdAsync(id);
            if (trip == null) return NotFound();
            return Ok(trip);
        }

        [HttpGet("ByRoute/{routeId}")]
        public async Task<IActionResult> GetTripsByRoute(int routeId)
        {
            var trips = await _tripService.GetTripsByRouteIdAsync(routeId);
            return Ok(trips);
        }

        [HttpGet("paged")]
        public async Task<ActionResult<PagedResult<TripDetailsDTO>>> GetPagedTrips(
            [FromQuery] string? search,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var pagedTrips = await _tripService.GetPagedTripsAsync(search, pageNumber, pageSize);
            return Ok(pagedTrips);
        }

        [HttpPost("{id}/start")]
        public async Task<IActionResult> StartTrip(int id)
        {
            try
            {
                var updatedTrip = await _tripService.StartTripAsync(id);
                return Ok(updatedTrip);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = "Error", Message = ex.Message });
            }
        }

        [HttpPost("{id}/complete")]
        public async Task<IActionResult> CompleteTrip(int id)
        {
            try
            {
                var updatedTrip = await _tripService.CompleteTripAsync(id);
                return Ok(updatedTrip);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = "Error", Message = ex.Message });
            }
        }

        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchTrips(
            [FromQuery] string? from,
            [FromQuery] string? to,
            [FromQuery] DateTime? date,
            [FromQuery] int passengers = 1)
        {
            var results = await _tripService.SearchTripsAsync(from, to, date, passengers);
            return Ok(results);
        }
    }
}
