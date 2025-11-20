using Microsoft.EntityFrameworkCore;
using SmartTransportation.BLL.DTOs.Booking;
using SmartTransportation.BLL.Exceptions;
using SmartTransportation.BLL.Interfaces;
using SmartTransportation.DAL.Models;
using SmartTransportation.DAL.Models.Common;
using SmartTransportation.DAL.Repositories.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartTransportation.BLL.Services
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BookingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // -------------------- GET --------------------
        public async Task<BookingResponseDto?> GetBookingByIdAsync(int bookingId)
        {
            var bookings = await _unitOfWork.Bookings.GetBookingsWithDetailsAsync();
            var booking = bookings.FirstOrDefault(b => b.BookingId == bookingId);
            if (booking == null) return null;

            return MapToResponseDto(booking);
        }

        public async Task<IEnumerable<BookingResponseDto>> GetAllBookingsAsync()
        {
            var bookings = await _unitOfWork.Bookings.GetBookingsWithDetailsAsync();
            return bookings.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<BookingResponseDto>> GetBookingsByUserIdAsync(int userId)
        {
            var bookings = await _unitOfWork.Bookings.GetBookingsWithDetailsAsync();
            var userBookings = bookings.Where(b => b.BookerUserId == userId).ToList();
            return userBookings.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<BookingResponseDto>> GetBookingsByTripIdAsync(int tripId)
        {
            var bookings = await _unitOfWork.Bookings.GetBookingsWithDetailsAsync();
            var tripBookings = bookings.Where(b => b.TripId == tripId).ToList();
            return tripBookings.Select(MapToResponseDto);
        }

        // -------------------- CREATE --------------------
        public async Task<BookingResponseDto> CreateBookingAsync(CreateBookingDto createDto)
        {
            // Validate trip exists
            var trip = await _unitOfWork.Trips.GetByIdAsync(createDto.TripId)
                ?? throw new NotFoundException("Trip", createDto.TripId);

            // Validate user exists
            var user = await _unitOfWork.Users.GetByIdAsync(createDto.BookerUserId)
                ?? throw new NotFoundException("User", createDto.BookerUserId);

            // Check available seats
            if (trip.AvailableSeats < createDto.SeatsCount)
                throw new ValidationException("SeatsCount", "Not enough available seats for this trip.");

            // Create booking
            var booking = new Booking
            {
                TripId = createDto.TripId,
                BookerUserId = createDto.BookerUserId,
                BookingDate = DateTime.UtcNow,
                BookingStatus = "Pending",
                PaymentStatus = "Pending",
                TotalAmount = 0m,
                SeatsCount = createDto.SeatsCount
            };


            await _unitOfWork.Bookings.AddAsync(booking);
            await _unitOfWork.SaveAsync();

            // Add passengers if provided
            if (createDto.PassengerUserIds != null && createDto.PassengerUserIds.Any())
            {
                foreach (var passengerId in createDto.PassengerUserIds)
                {
                    var passenger = await _unitOfWork.Users.GetByIdAsync(passengerId);
                    if (passenger != null)
                    {
                        await _unitOfWork.BookingPassengers.AddAsync(new BookingPassenger
                        {
                            BookingId = booking.BookingId,
                            PassengerUserId = passengerId,
                            CheckInStatus = false
                        });
                    }
                }
            }

            // Add segments if provided
            if (createDto.SegmentIds != null && createDto.SegmentIds.Any())
            {
                foreach (var segmentId in createDto.SegmentIds)
                {
                    var segment = await _unitOfWork.RouteSegments.GetByIdAsync(segmentId);
                    if (segment != null)
                    {
                        await _unitOfWork.BookingSegments.AddAsync(new BookingSegment
                        {
                            BookingId = booking.BookingId,
                            SegmentId = segmentId
                        });
                    }
                }
            }

            // Update trip available seats
            trip.AvailableSeats -= createDto.SeatsCount;
            _unitOfWork.Trips.Update(trip);
            await _unitOfWork.SaveAsync();

            // Reload booking with details
            var bookings = await _unitOfWork.Bookings.GetBookingsWithDetailsAsync();
            var createdBooking = bookings.FirstOrDefault(b => b.BookingId == booking.BookingId);
            return MapToResponseDto(createdBooking!);
        }

        // -------------------- UPDATE --------------------
        public async Task<BookingResponseDto?> UpdateBookingAsync(int bookingId, UpdateBookingDto updateDto)
        {
            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId)
                ?? throw new NotFoundException("Booking", bookingId);

            // Update properties
            if (!string.IsNullOrEmpty(updateDto.BookingStatus))
                booking.BookingStatus = updateDto.BookingStatus;

            if (!string.IsNullOrEmpty(updateDto.PaymentStatus))
                booking.PaymentStatus = updateDto.PaymentStatus;

            if (updateDto.TotalAmount.HasValue)
                booking.TotalAmount = updateDto.TotalAmount.Value;

            if (updateDto.SeatsCount.HasValue)
            {
                var trip = await _unitOfWork.Trips.GetByIdAsync(booking.TripId);
                if (trip != null)
                {
                    var seatDifference = updateDto.SeatsCount.Value - booking.SeatsCount;
                    if (trip.AvailableSeats < seatDifference)
                        throw new ValidationException("SeatsCount", "Not enough available seats for this update.");

                    trip.AvailableSeats -= seatDifference;
                    _unitOfWork.Trips.Update(trip);
                }
                booking.SeatsCount = updateDto.SeatsCount.Value;
            }

            _unitOfWork.Bookings.Update(booking);
            await _unitOfWork.SaveAsync();

            var bookings = await _unitOfWork.Bookings.GetBookingsWithDetailsAsync();
            var updatedBooking = bookings.FirstOrDefault(b => b.BookingId == bookingId);
            return MapToResponseDto(updatedBooking!);
        }

        // -------------------- DELETE & CANCEL --------------------
        public async Task<bool> DeleteBookingAsync(int bookingId)
        {
            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId)
                ?? throw new NotFoundException("Booking", bookingId);

            var trip = await _unitOfWork.Trips.GetByIdAsync(booking.TripId);
            if (trip != null)
            {
                trip.AvailableSeats += booking.SeatsCount;
                _unitOfWork.Trips.Update(trip);
            }

            _unitOfWork.Bookings.Remove(booking);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> CancelBookingAsync(int bookingId)
        {
            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId)
                ?? throw new NotFoundException("Booking", bookingId);

            booking.BookingStatus = "Cancelled";

            var trip = await _unitOfWork.Trips.GetByIdAsync(booking.TripId);
            if (trip != null)
            {
                trip.AvailableSeats += booking.SeatsCount;
                _unitOfWork.Trips.Update(trip);
            }

            _unitOfWork.Bookings.Update(booking);
            await _unitOfWork.SaveAsync();
            return true;
        }

        // -------------------- PAGINATION --------------------
        public async Task<PagedResult<BookingResponseDto>> GetPagedBookingsAsync(string? search, int pageNumber, int pageSize)
        {
            // Start with IQueryable from repository
            var query = _unitOfWork.Bookings.QueryBookingsWithDetails(); // This should return IQueryable<Booking>

            // Apply search filter if provided
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b =>
                    (b.BookerUser != null && b.BookerUser.UserName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (b.Trip != null && b.Trip.Status.Contains(search, StringComparison.OrdinalIgnoreCase))
                );
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var bookings = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(b => b.BookerUser)
                .Include(b => b.Trip)
                .Include(b => b.BookingPassengers)
                    .ThenInclude(bp => bp.PassengerUser)
                .Include(b => b.BookingSegments)
                .ToListAsync();

            // Map to DTOs
            var items = bookings.Select(MapToResponseDto).ToList();

            return new PagedResult<BookingResponseDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        // -------------------- MAPPER --------------------
        private BookingResponseDto MapToResponseDto(Booking booking)
        {
            return new BookingResponseDto
            {
                BookingId = booking.BookingId,
                TripId = booking.TripId,
                BookerUserId = booking.BookerUserId,
                BookerUserName = booking.BookerUser?.UserName ?? string.Empty,
                BookingDate = booking.BookingDate,
                BookingStatus = booking.BookingStatus,
                PaymentStatus = booking.PaymentStatus,
                TotalAmount = booking.TotalAmount,
                SeatsCount = booking.SeatsCount,
                Trip = booking.Trip != null ? new TripInfoDto
                {
                    TripId = booking.Trip.TripId,
                    StartTime = booking.Trip.StartTime,
                    EndTime = booking.Trip.EndTime,
                    PricePerSeat = booking.Trip.PricePerSeat,
                    AvailableSeats = booking.Trip.AvailableSeats,
                    Status = booking.Trip.Status
                } : null,
                Passengers = booking.BookingPassengers?.Select(p => new BookingPassengerDto
                {
                    BookingPassengerId = p.BookingPassengerId,
                    PassengerUserId = p.PassengerUserId,
                    PassengerUserName = p.PassengerUser?.UserName ?? string.Empty,
                    SeatNumber = p.SeatNumber,
                    CheckInStatus = p.CheckInStatus
                }).ToList(),
                Segments = booking.BookingSegments?.Select(s => new BookingSegmentDto
                {
                    BookingSegmentId = s.BookingSegmentId,
                    SegmentId = s.SegmentId
                }).ToList()
            };
        }
    }
}
