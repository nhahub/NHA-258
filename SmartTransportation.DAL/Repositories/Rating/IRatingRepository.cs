using SmartTransportation.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartTransportation.DAL.Repositories
{
    public interface IRatingRepository : Generic.IGenericRepository<Rating>
    {
        Task<IEnumerable<Rating>> GetByTripIdAsync(int tripId);

        Task<IEnumerable<Rating>> GetForDriversAsync(IEnumerable<int> driverIds);
    }
}
