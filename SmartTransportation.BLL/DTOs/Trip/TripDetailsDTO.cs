using System;
using System.Collections.Generic;
using SmartTransportation.BLL.DTOs.Route;
using SmartTransportation.BLL.DTOs.Location;
using SmartTransportation.BLL.DTOs.Weather;

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

        // Trip-specific locations (stops, waypoints)
        public List<TripLocationDTO> TripLocations { get; set; } = new();

        // Calculated from segments if needed
        public decimal? TotalDistanceKm => Route?.TotalDistanceKm;
        public int? EstimatedTimeMinutes => Route?.EstimatedTimeMinutes;

        // Current or latest weather for trip start
        public WeatherDTO? LatestWeather => Route?.LatestWeather;

        // Optional: live driver location
        public MapLocationDTO? CurrentDriverLocation { get; set; }
    }
}
