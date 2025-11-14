using System;
using System.Collections.Generic;

namespace SmartTransportation.BLL.DTOs.Booking
{
    public class BookingResponseDto
    {
        public int BookingId { get; set; }
        public int TripId { get; set; }
        public int BookerUserId { get; set; }
        public string BookerUserName { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public string BookingStatus { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int SeatsCount { get; set; } 
        public TripInfoDto? Trip { get; set; }
        public List<BookingPassengerDto>? Passengers { get; set; }
        public List<BookingSegmentDto>? Segments { get; set; }
    }

    public class TripInfoDto
    {
        public int TripId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal PricePerSeat { get; set; }
        public int AvailableSeats { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class BookingPassengerDto
    {
        public int BookingPassengerId { get; set; }
        public int PassengerUserId { get; set; }
        public string PassengerUserName { get; set; } = string.Empty;
        public int? SeatNumber { get; set; }
        public bool CheckInStatus { get; set; }
    }

    public class BookingSegmentDto
    {
        public int BookingSegmentId { get; set; }
        public int SegmentId { get; set; }
    }
    
    public class CreateBookingDto
    {
        public int TripId { get; set; }
        public int BookerUserId { get; set; }
        public int SeatsCount { get; set; }
        public decimal TotalAmount { get; set; }
        public List<int>? PassengerUserIds { get; set; }
        public List<int>? SegmentIds { get; set; }
    }

    public class UpdateBookingDto
    {
        public string? BookingStatus { get; set; }
        public string? PaymentStatus { get; set; }
        public decimal? TotalAmount { get; set; }
        public int? SeatsCount { get; set; }
    }
}
