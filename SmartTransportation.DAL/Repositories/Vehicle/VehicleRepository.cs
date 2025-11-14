using Microsoft.EntityFrameworkCore;
using SmartTransportation.DAL.Models;
using SmartTransportation.DAL.Repositories.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartTransportation.DAL.Repositories
{
    public class VehicleRepository : GenericRepository<Vehicle>, IVehicleRepository
    {
        public VehicleRepository(TransportationContext context) : base(context) { }

        public async Task<IEnumerable<Vehicle>> GetVehiclesByDriverIdAsync(int driverId)
        {
            return await _dbSet.Where(v => v.DriverId == driverId).ToListAsync();
        }

        public async Task<Vehicle> GetByPlateNumberAsync(string plateNumber)
        {
            return await _dbSet.FirstOrDefaultAsync(v => v.PlateNumber == plateNumber);
        }
    }
}
