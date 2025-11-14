namespace SmartTransportation.BLL.DTOs.Route
{
    public class CreateSegmentDTO
    {
        public int SegmentOrder { get; set; }
        public string StartPoint { get; set; } = string.Empty;
        public string EndPoint { get; set; } = string.Empty;
    }
}