// Path: SmartTransportation.BLL/Gateways/GoogleMapsGateway.cs
// *** VERSION 14: Updated with all 27 Egyptian Governorates ***
using SmartTransportation.BLL.Interfaces;
using SmartTransportation.DAL.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SmartTransportation.BLL.Gateways
{
    public class GoogleMapsGateway : IGoogleMapsGateway
    {
        // --- Mock Geocoding Database ---
        // Contains coordinates for all 27 Egyptian Governorates
        private readonly Dictionary<string, (decimal Lat, decimal Lng)> _mockGeocodingDB = 
            new(StringComparer.OrdinalIgnoreCase)
        {
            // Delta
            {"Cairo", (30.0444m, 31.2357m)},
            {"Alexandria", (31.2001m, 29.9187m)},
            {"Giza", (29.9870m, 31.2118m)},
            {"Qalyubia", (30.4586m, 31.1833m)}, // Banha
            {"Menoufia", (30.5600m, 31.0078m)}, // Shibin
            {"Gharbia", (30.7865m, 31.0004m)}, // Tanta
            {"Kafr El Sheikh", (31.1129m, 30.9388m)},
            {"Dakahlia", (31.0364m, 31.3807m)}, // Mansoura
            {"Sharqia", (30.5877m, 31.5021m)}, // Zagazig
            {"Damietta", (31.4165m, 31.8133m)},
            {"Beheira", (31.0381m, 30.4679m)}, // Damanhur

            // Canal
            {"Port Said", (31.2653m, 32.3019m)},
            {"Ismailia", (30.6043m, 32.2723m)},
            {"Suez", (29.9737m, 32.5263m)},

            // Upper Egypt
            {"Faiyum", (29.3090m, 30.8425m)},
            {"Beni Suef", (29.0748m, 31.0978m)},
            {"Minya", (28.1099m, 30.7503m)},
            {"Asyut", (27.1783m, 31.1859m)},
            {"Sohag", (26.5569m, 31.6948m)},
            {"Qena", (26.1623m, 32.7266m)},
            {"Luxor", (25.6872m, 32.6396m)},
            {"Aswan", (24.0889m, 32.8998m)},

            // Border
            {"Red Sea", (27.2578m, 33.8116m)}, // Hurghada
            {"New Valley", (25.4452m, 30.5451m)}, // Kharga
            {"Matrouh", (31.3543m, 27.2454m)}, // Mersa Matrouh
            {"North Sinai", (31.1346m, 33.8036m)}, // Arish
            {"South Sinai", (28.2366m, 33.6231m)} // El Tor
        };
        // ------------------------------------

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public GoogleMapsGateway(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<RouteCalculationResult> FetchRouteDetailsAsync(
            string startLocation, 
            string endLocation, 
            int routeId, 
            int segmentId)
        {
            await Task.Delay(20); // Simulate network delay

            var (startLat, startLng) = _mockGeocodingDB.TryGetValue(startLocation, out var start) 
                ? start : (30.0m, 31.0m);
            var (endLat, endLng) = _mockGeocodingDB.TryGetValue(endLocation, out var end) 
                ? end : (31.0m, 30.0m);

            var distance = CalculateHaversineDistance(startLat, startLng, endLat, endLng);
            var time = (int)((distance / 80.0m) * 60.0m);

            var locations = new List<MapLocation>
            {
                new()
                {
                    SegmentId = segmentId,
                    Latitude = startLat,
                    Longitude = startLng,
                    Description = $"Start: {startLocation}",
                    StopOrder = 1,
                    GoogleAddress = $"Mocked Address for {startLocation}"
                },
                new()
                {
                    SegmentId = segmentId,
                    Latitude = endLat,
                    Longitude = endLng,
                    Description = $"End: {endLocation}",
                    StopOrder = 2,
                    GoogleAddress = $"Mocked Address for {endLocation}"
                }
            };

            return new RouteCalculationResult
            {
                TotalDistanceKm = Math.Round(distance, 2),
                EstimatedTimeMinutes = time,
                MapLocations = locations
            };
        }

        private static decimal CalculateHaversineDistance(
            decimal lat1, decimal lon1, 
            decimal lat2, decimal lon2)
        {
            const decimal R = 6371; // Earth's radius in km

            decimal dLat = (decimal)Math.PI * (lat2 - lat1) / 180;
            decimal dLon = (decimal)Math.PI * (lon2 - lon1) / 180;

            decimal a = 
                (decimal)Math.Sin((double)dLat / 2) * (decimal)Math.Sin((double)dLat / 2) +
                (decimal)Math.Cos((double)((decimal)Math.PI * lat1 / 180)) * 
                (decimal)Math.Cos((double)((decimal)Math.PI * lat2 / 180)) *
                (decimal)Math.Sin((double)dLon / 2) * (decimal)Math.Sin((double)dLon / 2);
            
            decimal c = 2 * (decimal)Math.Asin(Math.Min(1, Math.Sqrt((double)a)));
            
            return R * c;
        }
    }
}