using System;

namespace SmartTransportation.BLL.DTOs.Trip
{
    public class CreateTripDTO
    {
        public int RouteId { get; set; }
        public int DriverId { get; set; }
        public DateTime StartTime { get; set; }
        public decimal PricePerSeat { get; set; }
        public int AvailableSeats { get; set; }
        public string? Notes { get; set; }
    }
}