// Path: SmartTransportation.BLL/Interfaces/IGoogleMapsGateway.cs
// *** VERSION 13.2: Interface for segment calculation ***
using SmartTransportation.BLL.DTOs.Location;
using SmartTransportation.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartTransportation.BLL.Interfaces
{
    /// <summary>
    /// Interface for a gateway to calculate route and geographic data.
    /// </summary>
    public interface IGoogleMapsGateway
    {
        /// <summary>
        /// Fetches the details for a single route segment (e.g., "Cairo" to "Tanta").
        /// In a real implementation, this would call the Google Directions API.
        /// </summary>
        Task<RouteCalculationResult> FetchRouteDetailsAsync(
            string startLocation, 
            string endLocation, 
            int routeId, 
            int segmentId);
    }

    public class RouteCalculationResult
    {
        public decimal TotalDistanceKm { get; set; }
        public int EstimatedTimeMinutes { get; set; }
        public List<MapLocation> MapLocations { get; init; } = new();
    }
}