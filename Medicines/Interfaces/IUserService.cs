using Medicines.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medicines.Interfaces
{
    public interface IUserService
    {
        Task<User?> GetUserByUserIdAsync(long userId);

        Task<bool> AddUserAsync(long userId, string username);

        Task<bool> UpdateUserAsync(long userId, string username);

        Task<bool> DeleteUserAsync(long userId);

        Task<IEnumerable<User>> GetAllUsersAsync();
    }
}
