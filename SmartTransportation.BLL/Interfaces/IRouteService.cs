// Path: SmartTransportation.BLL/Interfaces/IRouteService.cs
using SmartTransportation.BLL.DTOs.Route;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartTransportation.BLL.Interfaces
{
    public interface IRouteService
    {
        Task<RouteDetailsDTO> CreateRouteAsync(CreateRouteDTO routeDto);
        Task<RouteDetailsDTO?> GetRouteDetailsByIdAsync(int routeId);
        Task<IEnumerable<RouteDetailsDTO>> GetAllRoutesAsync();
    }
}