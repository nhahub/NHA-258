using SmartTransportation.DAL.Models;
using SmartTransportation.DAL.Repositories.Generic;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartTransportation.DAL.Repositories
{
    public interface IMapLocationRepository : IGenericRepository<MapLocation>
    {
        Task<IEnumerable<MapLocation>> GetBySegmentIdAsync(int segmentId);
    }
}
