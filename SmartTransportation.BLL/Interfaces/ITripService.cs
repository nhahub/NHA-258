// Path: SmartTransportation.BLL/Interfaces/ITripService.cs
// *** VERSION 18: Refactored using Trip namespace ***
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartTransportation.BLL.DTOs.Trip;

namespace SmartTransportation.BLL.Interfaces
{
    public interface ITripService
    {
        Task<TripDetailsDTO> CreateTripAsync(CreateTripDTO tripDto);
        Task<TripDetailsDTO?> GetTripDetailsByIdAsync(int tripId);
        Task<IEnumerable<TripDetailsDTO>> GetTripsByRouteIdAsync(int routeId);
        Task<TripDetailsDTO> StartTripAsync(int tripId);
        Task<TripDetailsDTO> CompleteTripAsync(int tripId);
    }
}