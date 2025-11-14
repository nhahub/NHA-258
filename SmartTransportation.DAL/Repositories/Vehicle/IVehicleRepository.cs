using SmartTransportation.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartTransportation.DAL.Repositories
{
    public interface IVehicleRepository : Generic.IGenericRepository<Vehicle>
    {
        Task<IEnumerable<Vehicle>> GetVehiclesByDriverIdAsync(int driverId);
        Task<Vehicle> GetByPlateNumberAsync(string plateNumber);
    }
}
