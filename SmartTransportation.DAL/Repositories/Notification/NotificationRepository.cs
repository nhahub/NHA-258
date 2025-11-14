using Microsoft.EntityFrameworkCore;
using SmartTransportation.DAL.Models;
using SmartTransportation.DAL.Repositories.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartTransportation.DAL.Repositories
{
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(TransportationContext context) : base(context) { }

        public async Task<IEnumerable<Notification>> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(n => n.User)
                .Where(n => n.UserId == userId)
                .ToListAsync();
        }

        public async Task<Notification?> GetByIdWithDetailsAsync(int notificationId)
        {
            return await _dbSet
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId);
        }
    }
}
