using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTransportation.BLL.DTOs.Trip;
using SmartTransportation.BLL.Interfaces;
using SmartTransportation.DAL.Models.Common;
using System.Threading.Tasks;
using System;

namespace SmartTransportation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TripsController : ControllerBase
    {
        private readonly ITripService _tripService;

        public TripsController(ITripService tripService)
        {
            _tripService = tripService;
        }

        // =======================================
        // ⭐ PUBLIC SEARCH ENDPOINT (used by Web)
        // GET: api/trips/search?from=..&to=..&date=..&passengers=..
        // =======================================
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


        // =======================================
        // GET: api/trips/{id}
        // Used by TripDetails.cshtml
        // =======================================
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTripById(int id)
        {
            var trip = await _tripService.GetTripDetailsByIdAsync(id);
            if (trip == null) return NotFound();

            return Ok(trip);
        }

        // =======================================
        // GET: api/Trips/ByRoute/{routeId}
        // =======================================
        [HttpGet("ByRoute/{routeId}")]
        [Authorize]
        public async Task<IActionResult> GetTripsByRoute(int routeId)
        {
            var trips = await _tripService.GetTripsByRouteIdAsync(routeId);
            return Ok(trips);
        }

        // =======================================
        // GET: api/Trips/paged
        // =======================================
        [HttpGet("paged")]
        [Authorize]
        public async Task<ActionResult<PagedResult<TripDetailsDTO>>> GetPagedTrips(
            [FromQuery] string? search,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var pagedTrips = await _tripService.GetPagedTripsAsync(search, pageNumber, pageSize);
            return Ok(pagedTrips);
        }

        // =======================================
        // POST: api/Trips (Driver/Admin only)
        // =======================================
        [HttpPost]
        [Authorize(Roles = "Driver,Admin")]
        public async Task<IActionResult> CreateTrip([FromBody] CreateTripDTO tripDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Error = "BadRequest", Message = "Invalid trip data.", Details = ModelState });

            try
            {
                var newTrip = await _tripService.CreateTripAsync(tripDto);
                return CreatedAtAction(nameof(GetTripById), new { id = newTrip.TripId }, newTrip);
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode(403, new { Error = "Forbidden", Message = "You do not have permission to create a trip." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = "Error", Message = ex.Message });
            }
        }

        // =======================================
        // POST: api/Trips/{id}/start
        // =======================================
        [HttpPost("{id}/start")]
        [Authorize(Roles = "Driver,Admin")]
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

        // =======================================
        // POST: api/Trips/{id}/complete
        // =======================================
        [HttpPost("{id}/complete")]
        [Authorize(Roles = "Driver,Admin")]
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
    }
}

