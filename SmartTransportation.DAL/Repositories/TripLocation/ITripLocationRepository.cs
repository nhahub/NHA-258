using SmartTransportation.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartTransportation.DAL.Repositories
{
    public interface ITripLocationRepository : Generic.IGenericRepository<TripLocation>
    {
        Task<IEnumerable<TripLocation>> GetLocationsByTripIdAsync(int tripId);
    }
}
