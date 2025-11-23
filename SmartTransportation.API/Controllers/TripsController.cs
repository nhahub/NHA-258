using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTransportation.BLL.DTOs.Trip;
using SmartTransportation.BLL.Interfaces;
using System.Threading.Tasks;
using System;

namespace SmartTransportation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TripsController : BaseApiController
    {
        private readonly ITripService _tripService;

        public TripsController(ITripService tripService)
        {
            _tripService = tripService;
        }

        [HttpPost]
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> CreateTrip([FromBody] CreateTripDTO tripDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Error = "BadRequest", Message = "Invalid trip data." });

            if (CurrentUserId == null)
                return Unauthorized(new { Error = "Unauthorized", Message = "Driver not found in token." });

            try
            {
                var newTrip = await _tripService.CreateTripAsync(tripDto, CurrentUserId.Value);
                return CreatedAtAction(nameof(GetTripById), new { id = newTrip.TripId }, newTrip);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = "Error", Message = ex.Message });
            }
        }

        [HttpGet("{id}")]
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

        // ⭐ Search Trips Endpoint
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

        [HttpPost("{id}/start")]
        [Authorize(Roles = "Driver")]
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
        [Authorize(Roles = "Driver")]
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
