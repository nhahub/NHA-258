using SmartTransportation.BLL.DTOs.Booking;
using SmartTransportation.DAL.Models.Common;

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
    Task<BookingResponseDto> CreateBookingAsync(CreateBookingDto createDto);
    Task<BookingResponseDto?> UpdateBookingAsync(int bookingId, UpdateBookingDto updateDto);
    Task<bool> DeleteBookingAsync(int bookingId);
    Task<bool> CancelBookingAsync(int bookingId);
}
