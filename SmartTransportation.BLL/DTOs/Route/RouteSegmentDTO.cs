public class RouteSegmentDTO
{
    public int SegmentOrder { get; set; }
    public string StartPoint { get; set; } = string.Empty;
    public string EndPoint { get; set; } = string.Empty;
    public decimal? SegmentDistanceKm { get; set; }      // <-- DTO
    public int? SegmentEstimatedMinutes { get; set; }   // <-- DTO
}
