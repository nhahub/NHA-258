// Path: SmartTransportation/Controllers/TripsController.cs
// *** VERSION 17: Added Lifecycle Endpoints ***
using Microsoft.AspNetCore.Mvc;
using SmartTransportation.BLL.DTOs.Trip;
using SmartTransportation.BLL.DTOs.Route;
using SmartTransportation.BLL.Interfaces;
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

        [HttpPost]
        public async Task<IActionResult> CreateTrip([FromBody] CreateTripDTO tripDto)
        {
            try
            {
                var newTrip = await _tripService.CreateTripAsync(tripDto);
                return CreatedAtAction(nameof(GetTripById), new { id = newTrip.TripId }, newTrip);
            }
            catch (Exception ex)
            {
                // This will now catch "Route not found" or "Driver not found"
                return BadRequest(ex.Message);
            }
        }

        // --- START OF NEW ENDPOINTS ---

        [HttpPost("{id}/start")] // e.g., POST /api/Trips/12/start
        public async Task<IActionResult> StartTrip(int id)
        {
            try
            {
                var updatedTrip = await _tripService.StartTripAsync(id);
                return Ok(updatedTrip);
            }
            catch (Exception ex)
            {
                // Returns 400 if trip not found or status is wrong
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/complete")] // e.g., POST /api/Trips/12/complete
        public async Task<IActionResult> CompleteTrip(int id)
        {
            try
            {
                var updatedTrip = await _tripService.CompleteTripAsync(id);
                return Ok(updatedTrip);
            }
            catch (Exception ex)
            {
                // Returns 400 if trip not found or status is wrong
                return BadRequest(ex.Message);
            }
        }
        
        // --- END OF NEW ENDPOINTS ---
    }
}