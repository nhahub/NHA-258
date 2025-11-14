using SmartTransportation.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartTransportation.DAL.Repositories
{
    public interface IBookingPassengerRepository : Generic.IGenericRepository<BookingPassenger>
    {
        Task<IEnumerable<BookingPassenger>> GetByBookingIdAsync(int bookingId);
    }
}
