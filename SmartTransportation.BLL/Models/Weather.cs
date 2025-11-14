using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartTransportation.DAL.Models;  // Add this to reference DAL models

namespace SmartTransportation.BLL.Models
{
    public class Weather
    {
        [Column("WeatherID")]
        public int WeatherId { get; set; }

        [Column("RouteID")]
        public int RouteId { get; set; }

        public DateOnly WeatherDate { get; set; }

        [Column(TypeName = "decimal(5, 2)")]
        public decimal? Temperature { get; set; }

        [StringLength(100)]
        public string? Condition { get; set; }

        [Column(TypeName = "decimal(5, 2)")]
        public decimal? Humidity { get; set; }

        [Column(TypeName = "decimal(5, 2)")]
        public decimal? WindSpeed { get; set; }

        [ForeignKey("RouteId")]
        public virtual Route? Route { get; set; }
    }
}