using SmartTransportation.DAL.Models;
using System.Threading.Tasks;

namespace SmartTransportation.DAL.Repositories
{
    public interface IUserProfileRepository : Generic.IGenericRepository<UserProfile>
    {
        Task<UserProfile> GetByUserIdAsync(int userId);
    }
}
