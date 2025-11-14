using SmartTransportation.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartTransportation.DAL.Repositories
{
    public interface IRouteRepository : Generic.IGenericRepository<Route>
    {
        Task<Route> GetRouteWithSegmentsAsync(int routeId);
    }
}
