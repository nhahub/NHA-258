using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartTransportation.DAL.Models;

namespace SmartTransportation.BLL.Models
{
    public class MapLocation
    {
        [Key]
        [Column("LocationID")]
        public int LocationId { get; set; }

        [Column("SegmentID")]
        public int SegmentId { get; set; }

        [Column(TypeName = "decimal(9, 6)")]
        public decimal? Latitude { get; set; }

        [Column(TypeName = "decimal(9, 6)")]
        public decimal? Longitude { get; set; }

        [StringLength(150)]
        public string? Description { get; set; }

        public int? StopOrder { get; set; }

        [Column("GooglePlaceID")]
        [StringLength(150)]
        public string? GooglePlaceId { get; set; }

        [StringLength(300)]
        public string? GoogleAddress { get; set; }

        [ForeignKey("SegmentId")]
        public virtual RouteSegment? Segment { get; set; }
    }
}