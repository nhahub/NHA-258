using System.Collections.Generic;
using System.Threading.Tasks;
using SmartTransportation.BLL.DTOs.Trip;
using SmartTransportation.DAL.Models.Common;

namespace SmartTransportation.BLL.Interfaces
{
    public interface ITripService
    {
        Task<TripDetailsDTO> CreateTripAsync(CreateTripDTO tripDto);
        Task<TripDetailsDTO?> GetTripDetailsByIdAsync(int tripId);
        Task<IEnumerable<TripDetailsDTO>> GetTripsByRouteIdAsync(int routeId);
        Task<TripDetailsDTO> StartTripAsync(int tripId);
        Task<TripDetailsDTO> CompleteTripAsync(int tripId);

        Task<List<TripSearchResultDto>> SearchTripsAsync(
            string? from,
            string? to,
            DateTime? date,
            int passengers);


        // Optional pagination
        Task<PagedResult<TripDetailsDTO>> GetPagedTripsAsync(
            string? search,
            int pageNumber,
            int pageSize
        );
    }
}
