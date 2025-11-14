using Microsoft.EntityFrameworkCore;
using SmartTransportation.DAL.Models;
using SmartTransportation.DAL.Repositories.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartTransportation.DAL.Repositories
{
    public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(TransportationContext context) : base(context) { }

        public async Task<IEnumerable<Payment>> GetByBookingIdAsync(int bookingId)
        {
            return await _dbSet
                .Where(p => p.BookingId == bookingId)
                .ToListAsync();
        }
    }
}
