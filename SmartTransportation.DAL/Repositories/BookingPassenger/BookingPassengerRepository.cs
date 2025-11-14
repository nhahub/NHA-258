using Microsoft.EntityFrameworkCore;
using SmartTransportation.DAL.Models;
using SmartTransportation.DAL.Repositories.Generic;


namespace SmartTransportation.DAL.Repositories
{
    public class BookingPassengerRepository : GenericRepository<BookingPassenger>, IBookingPassengerRepository
    {
        public BookingPassengerRepository(TransportationContext context) : base(context) { }

        public async Task<IEnumerable<BookingPassenger>> GetByBookingIdAsync(int bookingId)
        {
            return await _dbSet
                .Where(bp => bp.BookingId == bookingId)
                .Include(bp => bp.PassengerUser)
                .ToListAsync();
        }
    }
}
