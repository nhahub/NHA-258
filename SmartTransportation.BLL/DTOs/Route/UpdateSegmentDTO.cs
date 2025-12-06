using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTransportation.BLL.DTOs.Route
{
    public class UpdateSegmentDTO
    {
        public int? SegmentId { get; set; }  // If null → create new
        public int SegmentOrder { get; set; }
        public string? StartPoint { get; set; }
        public string? EndPoint { get; set; }
        public decimal SegmentDistanceKm { get; set; }
        public int SegmentEstimatedMinutes { get; set; }
    }
}
