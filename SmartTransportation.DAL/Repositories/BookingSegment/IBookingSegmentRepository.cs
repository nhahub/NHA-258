using SmartTransportation.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartTransportation.DAL.Repositories
{
    public interface IBookingSegmentRepository : Generic.IGenericRepository<BookingSegment>
    {
        Task<IEnumerable<BookingSegment>> GetByBookingIdAsync(int bookingId);
    }
}
