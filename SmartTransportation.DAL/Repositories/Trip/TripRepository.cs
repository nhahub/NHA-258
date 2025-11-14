using Microsoft.EntityFrameworkCore;
using SmartTransportation.DAL.Models;
using SmartTransportation.DAL.Repositories.Generic;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartTransportation.DAL.Repositories
{
    public class TripRepository : GenericRepository<Trip>, ITripRepository
    {
        public TripRepository(TransportationContext context) : base(context) { }

        public async Task<IEnumerable<Trip>> GetTripsByRouteIdAsync(int routeId)
        {
            return await _dbSet
                .Where(t => t.RouteId == routeId)
                .Include(t => t.Bookings)
                .Include(t => t.Ratings)
                .Include(t => t.TripLocations)
                .ToListAsync();
        }

        public async Task<IEnumerable<Trip>> GetTripsByDriverIdAsync(int driverId)
        {
            return await _dbSet
                .Where(t => t.DriverId == driverId)
                .Include(t => t.Bookings)
                .Include(t => t.Ratings)
                .Include(t => t.TripLocations)
                .ToListAsync();
        }
    }
}
