using Microsoft.EntityFrameworkCore;
using SmartTransportation.DAL.Models;
using SmartTransportation.DAL.Repositories.Generic;
namespace SmartTransportation.DAL.Repositories
{
    public class RouteSegmentRepository : GenericRepository<RouteSegment>, IRouteSegmentRepository
    {
        public RouteSegmentRepository(TransportationContext context) : base(context) { }

        public async Task<IEnumerable<RouteSegment>> GetByRouteIdAsync(int routeId)
        {
            return await _dbSet
                .Where(rs => rs.RouteId == routeId)
                .Include(rs => rs.MapLocations)
                .ToListAsync();
        }
    }
}
