using Medicines.Context;
using Medicines.Interfaces;
using Medicines.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medicines.Repository
{
    public class UserRepository : RepositoryBase<User>, IUserRepository
    {
        public UserRepository(RepositoryContext context) : base(context)
        {

        }

        public void AddUser(User user) => Add(user);

        public void DeleteUser(User user) => Delete(user);

        public Task<Models.User?> GetUserByIdAsync(Guid id)
        {
            return FindByCondition(u => u.Id == id).FirstOrDefaultAsync();
        }

        public Task<Models.User?> GetUserByUserIdAsync(long userId)
        {
            return FindByCondition(u => u.UserId == userId).FirstOrDefaultAsync();
        }

        public void UpdateUser(User user) => Update(user);
        public Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return FindAll().
                    ToListAsync().
                    ContinueWith(FindAll => FindAll.Result.AsEnumerable());
        }
    }
}
