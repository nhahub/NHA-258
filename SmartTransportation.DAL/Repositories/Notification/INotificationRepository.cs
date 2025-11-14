using SmartTransportation.DAL.Models;
using SmartTransportation.DAL.Repositories.Generic;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartTransportation.DAL.Repositories
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<IEnumerable<Notification>> GetByUserIdAsync(int userId);
        Task<Notification?> GetByIdWithDetailsAsync(int notificationId);
    }
}
