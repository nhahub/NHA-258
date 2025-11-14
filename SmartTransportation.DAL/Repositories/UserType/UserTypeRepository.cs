using Microsoft.EntityFrameworkCore;
using SmartTransportation.DAL.Models;
using SmartTransportation.DAL.Repositories.Generic;
using System.Threading.Tasks;

namespace SmartTransportation.DAL.Repositories
{
    public class UserTypeRepository : GenericRepository<UserType>, IUserTypeRepository
    {
        public UserTypeRepository(TransportationContext context) : base(context) { }

        public async Task<UserType> GetByNameAsync(string typeName)
        {
            return await _dbSet.FirstOrDefaultAsync(ut => ut.TypeName == typeName);
        }
    }
}

