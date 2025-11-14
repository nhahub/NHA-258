using Microsoft.EntityFrameworkCore;
using SmartTransportation.DAL.Models;
using SmartTransportation.DAL.Repositories.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartTransportation.DAL.Repositories
{
    public class BookingSegmentRepository : GenericRepository<BookingSegment>, IBookingSegmentRepository
    {
        public BookingSegmentRepository(TransportationContext context) : base(context) { }

        public async Task<IEnumerable<BookingSegment>> GetByBookingIdAsync(int bookingId)
        {
            return await _dbSet
                .Where(bs => bs.BookingId == bookingId)
                .Include(bs => bs.Segment)
                .ToListAsync();
        }
    }
}
