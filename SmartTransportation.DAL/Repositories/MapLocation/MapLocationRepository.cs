using Microsoft.EntityFrameworkCore;
using SmartTransportation.DAL.Models;
using SmartTransportation.DAL.Repositories.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartTransportation.DAL.Repositories
{
    public class MapLocationRepository : GenericRepository<MapLocation>, IMapLocationRepository
    {
        public MapLocationRepository(TransportationContext context) : base(context) { }

        public async Task<IEnumerable<MapLocation>> GetBySegmentIdAsync(int segmentId)
        {
            return await _dbSet
                .Where(ml => ml.SegmentId == segmentId)
                .ToListAsync();
        }
    }
}
