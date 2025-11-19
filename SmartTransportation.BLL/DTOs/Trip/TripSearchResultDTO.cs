using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SmartTransportation.BLL.DTOs.Trip
{
    public class TripSearchResultDto
    {
        public int TripId { get; set; }
        public string FromLocation { get; set; }
        public string ToLocation { get; set; }
        public DateTime DepartureDate { get; set; }
        public string DepartureTime { get; set; }
        public int MaxPassengers { get; set; }
        public int AvailableSeats { get; set; }
        public int NumberOfBookings { get; set; }   // NEW
        public decimal Price { get; set; }
        public string VehicleType { get; set; }
        public string DriverName { get; set; }
        public double DriverRating { get; set; }
        public int TotalReviews { get; set; }
    }
}

