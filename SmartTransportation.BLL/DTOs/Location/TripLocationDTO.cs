using System;

namespace SmartTransportation.BLL.DTOs.Location
{
    public class TripLocationDTO
    {
        public int TripLocationId { get; set; }
        public int TripId { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? Speed { get; set; }
        public decimal? Heading { get; set; }
        public string? GooglePlaceId { get; set; }
        public string? GoogleAddress { get; set; }
        public DateTime RecordedAt { get; set; }
    }
}