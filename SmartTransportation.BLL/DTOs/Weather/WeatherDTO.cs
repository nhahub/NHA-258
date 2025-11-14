using System;

namespace SmartTransportation.BLL.DTOs.Weather
{
    public class WeatherDTO
    {
        public int WeatherId { get; set; }
        public int RouteId { get; set; }
        public DateOnly WeatherDate { get; set; }
        public decimal? Temperature { get; set; }
        public string? Condition { get; set; }
        public decimal? WindSpeed { get; set; }
        public decimal? Humidity { get; set; }
    }
}