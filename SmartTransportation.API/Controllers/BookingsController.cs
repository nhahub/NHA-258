using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTransportation.BLL.DTOs.Booking;
using SmartTransportation.BLL.Exceptions;
using SmartTransportation.BLL.Interfaces;
using SmartTransportation.DAL.Models.Common;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SmartTransportation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirst("UserId")
                        ?? User.FindFirst("UserID")
                        ?? User.FindFirst(ClaimTypes.NameIdentifier);

            if (claim != null && int.TryParse(claim.Value, out int userId))
                return userId;

            return null;
        }

        // ---------------- CREATE BOOKING (Passenger only) ----------------
        [HttpPost]
        [Authorize(Roles = "Passenger")]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    Error = "BadRequest",
                    Message = "Invalid booking data.",
                    Details = ModelState
                });

            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { Error = "Unauthorized", Message = "Invalid token or user ID." });

            try
            {
                var booking = await _bookingService.CreateBookingAsync(dto, userId.Value);
                return CreatedAtAction(nameof(GetBookingById), new { id = booking.BookingId }, booking);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = "Error", Message = ex.Message });
            }
        }

        // ---------------- GET BOOKING BY ID ----------------
        [HttpGet("{id}")]
        [AllowAnonymous] // زي TripsController
        public async Task<IActionResult> GetBookingById(int id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null) return NotFound();

            return Ok(booking);
        }

        // ---------------- GET BOOKINGS BY USER ----------------
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetBookingsByUser(int userId)
        {
            var bookings = await _bookingService.GetBookingsByUserIdAsync(userId);
            return Ok(bookings);
        }
        [HttpGet("user/{userId}/stats")]
        [Authorize(Roles = "Passenger")]
        public async Task<IActionResult> GetBookingStatsByUser(int userId)
        {
            var bookingsQuery = _bookingService.QueryBookingsByUserId(userId)
                                .Where(b => b.Trip != null && b.BookingStatus != "Cancelled");

            var total = await bookingsQuery.CountAsync();
            var upcoming = await bookingsQuery.CountAsync(b => b.Trip.StartTime.ToUniversalTime() > DateTime.UtcNow);
            var completed = await bookingsQuery.CountAsync(b => b.Trip.StartTime.ToUniversalTime() <= DateTime.UtcNow);

            return Ok(new
            {
                TotalBookings = total,
                UpcomingBookings = upcoming,
                CompletedBookings = completed
            });
        }


        // ---------------- GET BOOKINGS BY TRIP ----------------
        [HttpGet("trip/{tripId}")]
        [Authorize(Roles = "Driver,Admin")]
        public async Task<IActionResult> GetBookingsByTrip(int tripId)
        {
            var bookings = await _bookingService.GetBookingsByTripIdAsync(tripId);
            return Ok(bookings);
        }

        // ---------------- PAGED BOOKINGS (Admin) ----------------
        [HttpGet("paged")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PagedResult<BookingResponseDto>>> GetPagedBookings(
            [FromQuery] string? search,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var paged = await _bookingService.GetPagedBookingsAsync(search, pageNumber, pageSize);
            return Ok(paged);
        }

        // ---------------- CANCEL BOOKING (Passenger) ----------------
        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "Passenger")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { Error = "Unauthorized", Message = "Invalid token or user ID." });

            try
            {
                var result = await _bookingService.CancelBookingAsync(id);
                return Ok(new { Message = "Booking cancelled successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = "Error", Message = ex.Message });
            }
        }

        // ---------------- DELETE BOOKING (Admin) ----------------
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            try
            {
                var result = await _bookingService.DeleteBookingAsync(id);
                return Ok(new { Message = "Booking deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = "Error", Message = ex.Message });
            }
        }
    }
}
