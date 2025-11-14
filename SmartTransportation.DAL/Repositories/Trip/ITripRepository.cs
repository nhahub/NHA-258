using SmartTransportation.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartTransportation.DAL.Repositories
{
    public interface ITripRepository : Generic.IGenericRepository<Trip>
    {
        Task<IEnumerable<Trip>> GetTripsByRouteIdAsync(int routeId);
        Task<IEnumerable<Trip>> GetTripsByDriverIdAsync(int driverId);
    }
}
