using SmartTransportation.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace SmartTransportation.DAL.Repositories
{
    public interface IBookingRepository : Generic.IGenericRepository<Booking>
    {
        Task<IEnumerable<Booking>> GetBookingsWithDetailsAsync();
    }
}
