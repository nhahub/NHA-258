using SmartTransportation.BLL.DTOs.Route;
using SmartTransportation.DAL.Models.Common;

namespace SmartTransportation.BLL.Interfaces
{
    public interface IRouteService
    {
        Task<RouteDetailsDTO> CreateRouteAsync(CreateRouteDTO routeDto);
        Task<RouteDetailsDTO?> GetRouteDetailsByIdAsync(int routeId);
        Task<IEnumerable<RouteDetailsDTO>> GetAllRoutesAsync();
        Task<PagedResult<RouteDetailsDTO>> GetPagedRoutesAsync(string? search, int pageNumber, int pageSize);

        Task<bool> RouteExistsAsync(string start, string end);

        Task<RouteDetailsDTO?> UpdateRouteAsync(int routeId, UpdateRouteDTO dto);
        Task<bool> DeleteRouteAsync(int routeId);
    }

}
