using Microsoft.EntityFrameworkCore;
using SmartTransportation.DAL.Models;
using SmartTransportation.DAL.Repositories.Generic;
using System.Threading.Tasks;

namespace SmartTransportation.DAL.Repositories
{
    public class RouteRepository : GenericRepository<Route>, IRouteRepository
    {
        public RouteRepository(TransportationContext context) : base(context) { }

        public async Task<Route> GetRouteWithSegmentsAsync(int routeId)
        {
            return await _dbSet
                .Include(r => r.RouteSegments)
                .Include(r => r.Trips)
                .Include(r => r.Weathers)
                .FirstOrDefaultAsync(r => r.RouteId == routeId);
        }
    }
}
