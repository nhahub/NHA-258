using Microsoft.EntityFrameworkCore;
using SmartTransportation.DAL.Models;
using SmartTransportation.DAL.Repositories.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartTransportation.DAL.Repositories
{
    public class TripLocationRepository : GenericRepository<TripLocation>, ITripLocationRepository
    {
        public TripLocationRepository(TransportationContext context) : base(context) { }

        public async Task<IEnumerable<TripLocation>> GetLocationsByTripIdAsync(int tripId)
        {
            return await _dbSet
                .Where(tl => tl.TripId == tripId)
                .ToListAsync();
        }
    }
}
