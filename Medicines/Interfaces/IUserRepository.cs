using Medicines.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medicines.Interfaces
{
    public interface IUserRepository: IRepositoryBase<User>
    {
        Task<User?> GetUserByIdAsync(Guid id);
        Task<User?> GetUserByUserIdAsync(long userId);
        void AddUser(User user);
        void UpdateUser(User user);
        void DeleteUser(User user);
        Task<IEnumerable<User>> GetAllUsersAsync();
    }
}
