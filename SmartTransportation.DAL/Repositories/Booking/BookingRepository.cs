using Microsoft.EntityFrameworkCore;
using SmartTransportation.DAL.Models;
using SmartTransportation.DAL.Repositories.Generic;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartTransportation.DAL.Repositories
{
    public class BookingRepository : GenericRepository<Booking>, IBookingRepository
    {
        public BookingRepository(TransportationContext context) : base(context) { }

        public async Task<IEnumerable<Booking>> GetBookingsWithDetailsAsync()
        {
            return await _dbSet
                .Include(b => b.BookerUser)
                .Include(b => b.Trip)
                .Include(b => b.BookingPassengers)
                    .ThenInclude(bp => bp.PassengerUser)
                .Include(b => b.BookingSegments)
                .Include(b => b.Payments)
                .ToListAsync();
        }

        // New method for IQueryable
        public IQueryable<Booking> QueryBookingsWithDetails()
        {
            return _dbSet
                .Include(b => b.BookerUser)
                .Include(b => b.Trip)
                .Include(b => b.BookingPassengers)
                    .ThenInclude(bp => bp.PassengerUser)
                .Include(b => b.BookingSegments)
                .Include(b => b.Payments);
        }
    }
}
