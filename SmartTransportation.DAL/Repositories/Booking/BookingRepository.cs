using Microsoft.EntityFrameworkCore;
using SmartTransportation.DAL.Models;
using SmartTransportation.DAL.Repositories.Generic;


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
                .Include(b => b.BookingSegments)
                .Include(b => b.Payments)
                .ToListAsync();
        }
    }
}
