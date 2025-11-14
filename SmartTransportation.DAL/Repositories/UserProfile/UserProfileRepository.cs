using Microsoft.EntityFrameworkCore;
using SmartTransportation.DAL.Models;
using SmartTransportation.DAL.Repositories.Generic;
using System.Threading.Tasks;

namespace SmartTransportation.DAL.Repositories
{
    public class UserProfileRepository : GenericRepository<UserProfile>, IUserProfileRepository
    {
        public UserProfileRepository(TransportationContext context) : base(context) { }

        public async Task<UserProfile> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(up => up.User)
                .FirstOrDefaultAsync(up => up.UserId == userId);
        }
    }
}
