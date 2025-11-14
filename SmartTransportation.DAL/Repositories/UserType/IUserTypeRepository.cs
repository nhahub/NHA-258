using SmartTransportation.DAL.Models;
using System.Threading.Tasks;

namespace SmartTransportation.DAL.Repositories
{
    public interface IUserTypeRepository : Generic.IGenericRepository<UserType>
    {
        Task<UserType> GetByNameAsync(string typeName);
    }
}
