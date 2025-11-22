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
            var booking = await _unitOfWork.Bookings
                .QueryBookingsWithDetails()
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            return booking == null ? null : MapToResponseDto(booking);
        }

        public async Task<IEnumerable<BookingResponseDto>> GetAllBookingsAsync()
        {
            var bookings = await _unitOfWork.Bookings.QueryBookingsWithDetails().ToListAsync();
            return bookings.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<BookingResponseDto>> GetBookingsByUserIdAsync(int userId)
        {
            var bookings = await QueryBookingsByUserId(userId).ToListAsync();
            return bookings.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<BookingResponseDto>> GetBookingsByTripIdAsync(int tripId)
        {
            var bookings = await QueryBookingsByTripId(tripId).ToListAsync();
            return bookings.Select(MapToResponseDto);
        }

        // -------------------- CREATE --------------------
        public async Task<BookingResponseDto> CreateBookingAsync(CreateBookingDto createDto, int bookerUserId)
        {
            var trip = await _unitOfWork.Trips.GetByIdAsync(createDto.TripId)
                ?? throw new NotFoundException("Trip", createDto.TripId);

            _ = await _unitOfWork.Users.GetByIdAsync(bookerUserId)
                ?? throw new NotFoundException("User", bookerUserId);

            var passengerIds = (createDto.PassengerUserIds ?? new List<int>()).Distinct().ToList();

            if (!passengerIds.Contains(bookerUserId))
                passengerIds.Add(bookerUserId);

            if (createDto.SeatsCount < 1)
                throw new ValidationException("SeatsCount", "SeatsCount must be at least 1.");

            // Validate only registered passengers
            var validPassengerIds = new List<int>();
            foreach (var pid in passengerIds)
            {
                var user = await _unitOfWork.Users.GetByIdAsync(pid);
                if (user != null)
                    validPassengerIds.Add(pid);
            }

            if (createDto.SeatsCount < validPassengerIds.Count)
                throw new ValidationException("SeatsCount", "SeatsCount cannot be less than number of registered passengers.");

            if (trip.AvailableSeats < createDto.SeatsCount)
                throw new ValidationException("SeatsCount", "Not enough available seats for this trip.");

            // Create booking
            var booking = new Booking
            {
                TripId = createDto.TripId,
                BookerUserId = bookerUserId,
                BookingDate = DateTime.UtcNow,
                BookingStatus = "Pending",
                PaymentStatus = "Pending",
                TotalAmount = 0m,
                SeatsCount = createDto.SeatsCount
            };

            await _unitOfWork.Bookings.AddAsync(booking);
            await _unitOfWork.SaveAsync();

            // Add passengers
            foreach (var pid in validPassengerIds)
            {
                await _unitOfWork.BookingPassengers.AddAsync(new BookingPassenger
                {
                    BookingId = booking.BookingId,
                    PassengerUserId = pid
                });
            }

            // Add segments
            if (createDto.SegmentIds != null)
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

            trip.AvailableSeats -= createDto.SeatsCount;
            _unitOfWork.Trips.Update(trip);
            await _unitOfWork.SaveAsync();

            var createdBooking = await _unitOfWork.Bookings.QueryBookingsWithDetails()
                .FirstOrDefaultAsync(b => b.BookingId == booking.BookingId);

            return MapToResponseDto(createdBooking!);
        }

        // -------------------- QUERY --------------------
        public IQueryable<Booking> QueryBookingsByUserId(int userId)
            => _unitOfWork.Bookings.QueryBookingsWithDetails().Where(b => b.BookerUserId == userId);

        public IQueryable<Booking> QueryBookingsByTripId(int tripId)
            => _unitOfWork.Bookings.QueryBookingsWithDetails().Where(b => b.TripId == tripId);

        // -------------------- UPDATE --------------------
        public async Task<BookingResponseDto?> UpdateBookingAsync(int bookingId, UpdateBookingDto updateDto)
        {
            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId)
                ?? throw new NotFoundException("Booking", bookingId);

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
                    var seatDiff = updateDto.SeatsCount.Value - booking.SeatsCount;
                    if (trip.AvailableSeats < seatDiff)
                        throw new ValidationException("SeatsCount", "Not enough available seats for this update.");

                    trip.AvailableSeats -= seatDiff;
                    _unitOfWork.Trips.Update(trip);
                }
                booking.SeatsCount = updateDto.SeatsCount.Value;
            }

            _unitOfWork.Bookings.Update(booking);
            await _unitOfWork.SaveAsync();

            var updatedBooking = await _unitOfWork.Bookings.QueryBookingsWithDetails()
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            return MapToResponseDto(updatedBooking!);
        }

        // -------------------- DELETE --------------------
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

        // -------------------- CANCEL --------------------
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
            var query = _unitOfWork.Bookings.QueryBookingsWithDetails();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b =>
                    (b.BookerUser != null && b.BookerUser.UserName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (b.Trip != null && b.Trip.Status.Contains(search, StringComparison.OrdinalIgnoreCase))
                );
            }

            var totalCount = await query.CountAsync();
            var bookings = await query.Skip((pageNumber - 1) * pageSize)
                                      .Take(pageSize)
                                      .ToListAsync();

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
