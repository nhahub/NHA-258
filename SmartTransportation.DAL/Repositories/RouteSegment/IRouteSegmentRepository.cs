using SmartTransportation.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartTransportation.DAL.Repositories
{
    public interface IRouteSegmentRepository : Generic.IGenericRepository<RouteSegment>
    {
        Task<IEnumerable<RouteSegment>> GetByRouteIdAsync(int routeId);
    }
}
