namespace SmartTransportation.BLL.DTOs.Route
{
    public class CreateRouteDTO
    {
        public string RouteName { get; set; } = string.Empty;
        public string StartLocation { get; set; } = string.Empty;
        public string EndLocation { get; set; } = string.Empty;

        public string? RouteType { get; set; }
        public bool IsCircular { get; set; }

        public List<CreateSegmentDTO> Segments { get; set; } = new();
    }

}
