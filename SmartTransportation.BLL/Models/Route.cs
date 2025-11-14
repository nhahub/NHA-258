using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartTransportation.DAL.Models;

namespace SmartTransportation.BLL.Models
{
    public class Route
    {
        [Key]
        [Column("RouteID")]
        public int RouteId { get; set; }

        [Required]
        [StringLength(150)]
        public string RouteName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string StartLocation { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string EndLocation { get; set; } = string.Empty;

        [StringLength(50)]
        public string? RouteType { get; set; }

        public bool IsCircular { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal? TotalDistanceKm { get; set; }

        public int? EstimatedTimeMinutes { get; set; }

        public DateTime CreatedAt { get; set; }

        [InverseProperty("Route")]
        public virtual ICollection<RouteSegment> RouteSegments { get; set; } = new List<RouteSegment>();

        [InverseProperty("Route")]
        public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();

        [InverseProperty("Route")]
        public virtual ICollection<Weather> Weathers { get; set; } = new List<Weather>();
    }
}