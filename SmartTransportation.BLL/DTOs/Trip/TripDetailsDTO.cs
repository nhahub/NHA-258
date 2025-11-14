using System;
using System.Collections.Generic;
using SmartTransportation.BLL.DTOs.Route;
using SmartTransportation.BLL.DTOs.Location;
    
namespace SmartTransportation.BLL.DTOs.Trip
{
    public class TripDetailsDTO
    {
        public int TripId { get; set; }
        public int RouteId { get; set; }
        public int DriverId { get; set; }
        public string DriverName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal PricePerSeat { get; set; }
        public int AvailableSeats { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public RouteDetailsDTO? Route { get; set; }
        public List<TripLocationDTO> TripLocations { get; set; } = new();
    }
}