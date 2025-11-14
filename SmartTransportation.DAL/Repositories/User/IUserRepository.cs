using SmartTransportation.DAL.Models;
using System.Threading.Tasks;

namespace SmartTransportation.DAL.Repositories
{
    public interface IUserRepository : Generic.IGenericRepository<User>
    {
        Task<User> GetByEmailAsync(string email);
        Task<User> GetByUserNameAsync(string username);
    }
}
