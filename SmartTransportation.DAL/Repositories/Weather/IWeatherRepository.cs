using SmartTransportation.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartTransportation.DAL.Repositories
{
    public interface IWeatherRepository : Generic.IGenericRepository<Weather>
    {
        Task<IEnumerable<Weather>> GetByRouteIdAsync(int routeId);
        Task<Weather> GetByRouteAndDateAsync(int routeId, System.DateOnly date);
    }
}
