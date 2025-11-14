using System.Collections.Generic;
using SmartTransportation.BLL.DTOs.Location;

namespace SmartTransportation.BLL.DTOs.Route
{
    public class RouteSegmentDTO
    {
        public int SegmentId { get; set; }
        public int RouteId { get; set; }
        public int SegmentOrder { get; set; }
        public string StartPoint { get; set; } = string.Empty;
        public string EndPoint { get; set; } = string.Empty;
        public decimal? DistanceKm { get; set; }
        public int? EstimatedMinutes { get; set; }
    }
}