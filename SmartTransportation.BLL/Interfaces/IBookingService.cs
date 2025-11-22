using SmartTransportation.BLL.DTOs.Booking;
using SmartTransportation.DAL.Models;
using SmartTransportation.DAL.Models.Common;
using System.Linq;

public interface IBookingService
{
    Task<BookingResponseDto?> GetBookingByIdAsync(int bookingId);
    Task<IEnumerable<BookingResponseDto>> GetAllBookingsAsync();
    Task<PagedResult<BookingResponseDto>> GetPagedBookingsAsync(
        string? search,
        int pageNumber,
        int pageSize
    );
    Task<IEnumerable<BookingResponseDto>> GetBookingsByUserIdAsync(int userId);
    Task<IEnumerable<BookingResponseDto>> GetBookingsByTripIdAsync(int tripId);

    // New queryable methods
    IQueryable<Booking> QueryBookingsByUserId(int userId);
    IQueryable<Booking> QueryBookingsByTripId(int tripId);

    Task<BookingResponseDto> CreateBookingAsync(CreateBookingDto createDto, int bookerUserId);
    Task<BookingResponseDto?> UpdateBookingAsync(int bookingId, UpdateBookingDto updateDto);
    Task<bool> DeleteBookingAsync(int bookingId);
    Task<bool> CancelBookingAsync(int bookingId);
}
