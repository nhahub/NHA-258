using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SmartTransportation.BLL.DTOs.Trip
{
    public class TripSearchResultDTO
    {
        public int TripId { get; set; }

        public string FromLocation { get; set; } = string.Empty;
        public string ToLocation { get; set; } = string.Empty;

        public DateTime DepartureDate { get; set; }
        public string DepartureTime { get; set; } = string.Empty;

        public int MaxPassengers { get; set; }      // for now same as AvailableSeats
        public int AvailableSeats { get; set; }

        public decimal Price { get; set; }          // maps from PricePerSeat

        public string VehicleType { get; set; } = string.Empty; // placeholder for now

        public string DriverName { get; set; } = string.Empty;
        public double DriverRating { get; set; }
        public int TotalReviews { get; set; }
    }
}
