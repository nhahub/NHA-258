using Microsoft.EntityFrameworkCore;
using SmartTransportation.DAL.Models;
using SmartTransportation.DAL.Repositories.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartTransportation.DAL.Repositories
{
    public class RatingRepository : GenericRepository<Rating>, IRatingRepository
    {
        public RatingRepository(TransportationContext context) : base(context) { }

        public async Task<IEnumerable<Rating>> GetByTripIdAsync(int tripId)
        {
            return await _dbSet
                .Where(r => r.TripId == tripId)
                .Include(r => r.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<Rating>> GetForDriversAsync(IEnumerable<int> driverIds)
        {
            var driverIdsList = driverIds?.Distinct().ToList() ?? new List<int>();
            if (!driverIdsList.Any())
                return new List<Rating>();

            return await _dbSet
                .Include(r => r.Trip)                // we need Trip.DriverId 
                .Where(r => r.Score.HasValue &&
                            driverIdsList.Contains(r.Trip.DriverId))
                .ToListAsync();
        }
    }
}
