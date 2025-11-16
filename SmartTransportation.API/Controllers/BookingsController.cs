using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTransportation.BLL.DTOs.Booking;
using SmartTransportation.BLL.Exceptions;
using SmartTransportation.BLL.Interfaces;
using SmartTransportation.DAL.Models.Common;

namespace SmartTransportation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Require authentication for all actions
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        // ---------------- GET PAGED BOOKINGS ----------------
        [HttpGet("paged")]
        [Authorize(Roles = "Admin")] // Only Admin can see all bookings paged
        public async Task<ActionResult<PagedResult<BookingResponseDto>>> GetPagedBookings(
            [FromQuery] string? search,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var pagedBookings = await _bookingService.GetPagedBookingsAsync(search, pageNumber, pageSize);
                return Ok(pagedBookings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving paged bookings.", error = ex.Message });
            }
        }

        // ---------------- GET ALL BOOKINGS ----------------
        [HttpGet]
        [Authorize(Roles = "Admin")] // Only Admin can get all bookings
        public async Task<ActionResult<IEnumerable<BookingResponseDto>>> GetAllBookings()
        {
            try
            {
                var bookings = await _bookingService.GetAllBookingsAsync();
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving bookings.", error = ex.Message });
            }
        }

        // ---------------- GET BOOKING BY ID ----------------
        [HttpGet("{id}")]
        public async Task<ActionResult<BookingResponseDto>> GetBookingById(int id)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(id);
                if (booking == null)
                    return NotFound(new { message = $"Booking with ID {id} was not found." });

                return Ok(booking);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the booking.", error = ex.Message });
            }
        }

        // ---------------- GET BOOKINGS BY USER ----------------
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<BookingResponseDto>>> GetBookingsByUserId(int userId)
        {
            try
            {
                var bookings = await _bookingService.GetBookingsByUserIdAsync(userId);
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving user bookings.", error = ex.Message });
            }
        }

        // ---------------- GET BOOKINGS BY TRIP ----------------
        [HttpGet("trip/{tripId}")]
        [Authorize(Roles = "Admin,Driver")] // Only Admin or Driver can get bookings by trip
        public async Task<ActionResult<IEnumerable<BookingResponseDto>>> GetBookingsByTripId(int tripId)
        {
            try
            {
                var bookings = await _bookingService.GetBookingsByTripIdAsync(tripId);
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving trip bookings.", error = ex.Message });
            }
        }

        // ---------------- CREATE BOOKING ----------------
        [HttpPost]
        [Authorize(Roles = "Passenger")] // Only Passenger can create booking
        public async Task<ActionResult<BookingResponseDto>> CreateBooking([FromBody] CreateBookingDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var booking = await _bookingService.CreateBookingAsync(createDto);
                return CreatedAtAction(nameof(GetBookingById), new { id = booking.BookingId }, booking);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message, errors = ex.Errors });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the booking.", error = ex.Message });
            }
        }

        // ---------------- UPDATE BOOKING ----------------
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Passenger")] // Admin or Passenger can update booking
        public async Task<ActionResult<BookingResponseDto>> UpdateBooking(int id, [FromBody] UpdateBookingDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var booking = await _bookingService.UpdateBookingAsync(id, updateDto);
                if (booking == null)
                    return NotFound(new { message = $"Booking with ID {id} was not found." });

                return Ok(booking);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message, errors = ex.Errors });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the booking.", error = ex.Message });
            }
        }

        // ---------------- CANCEL BOOKING ----------------
        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "Passenger")] // Only Passenger can cancel their booking
        public async Task<ActionResult> CancelBooking(int id)
        {
            try
            {
                var result = await _bookingService.CancelBookingAsync(id);
                if (!result)
                    return NotFound(new { message = $"Booking with ID {id} was not found." });

                return Ok(new { message = "Booking cancelled successfully." });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while cancelling the booking.", error = ex.Message });
            }
        }

        // ---------------- DELETE BOOKING ----------------
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Only Admin can delete bookings
        public async Task<ActionResult> DeleteBooking(int id)
        {
            try
            {
                var result = await _bookingService.DeleteBookingAsync(id);
                if (!result)
                    return NotFound(new { message = $"Booking with ID {id} was not found." });

                return Ok(new { message = "Booking deleted successfully." });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the booking.", error = ex.Message });
            }
        }
    }
}
