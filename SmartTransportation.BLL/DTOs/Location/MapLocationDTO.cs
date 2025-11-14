namespace SmartTransportation.BLL.DTOs.Location
{
    public class MapLocationDTO
    {
        public int LocationId { get; set; }
        public int SegmentId { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? Description { get; set; }
        public int? StopOrder { get; set; }
        public string? GooglePlaceId { get; set; }
        public string? GoogleAddress { get; set; }
    }
}