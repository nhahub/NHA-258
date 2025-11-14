using Microsoft.EntityFrameworkCore;
using SmartTransportation.DAL.Models;
using SmartTransportation.DAL.Repositories.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartTransportation.DAL.Repositories
{
    public class WeatherRepository : GenericRepository<Weather>, IWeatherRepository
    {
        public WeatherRepository(TransportationContext context) : base(context) { }

        public async Task<IEnumerable<Weather>> GetByRouteIdAsync(int routeId)
        {
            return await _dbSet.Where(w => w.RouteId == routeId).ToListAsync();
        }

        public async Task<Weather> GetByRouteAndDateAsync(int routeId, System.DateOnly date)
        {
            return await _dbSet.FirstOrDefaultAsync(w => w.RouteId == routeId && w.WeatherDate == date);
        }
    }
}
