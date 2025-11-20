using System;
using System.Collections.Generic;
using SmartTransportation.BLL.DTOs.Location;
using SmartTransportation.BLL.DTOs.Weather;

namespace SmartTransportation.BLL.DTOs.Route
{
    public class RouteDetailsDTO
    {
        public int RouteId { get; set; }
        public string RouteName { get; set; } = string.Empty;
        public string StartLocation { get; set; } = string.Empty;
        public string EndLocation { get; set; } = string.Empty;
        public string? RouteType { get; set; }
        public bool IsCircular { get; set; }

        public decimal? TotalDistanceKm { get; set; }
        public int? EstimatedTimeMinutes { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<RouteSegmentDTO> Segments { get; set; } = new();
        public List<MapLocationDTO> MapLocations { get; set; } = new();

        public WeatherDTO? LatestWeather { get; set; }
    }
}
